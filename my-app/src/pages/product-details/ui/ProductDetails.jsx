import React, { Component } from 'react';
import { useParams } from 'react-router-dom';
import './ProductDetails.css';
import { withNavigate } from 'shared/lib/hoc/withNavigate';
import { CartContext } from 'entities/cart/model/CartContext';
import Header from 'widgets/header/ui/Header';
import MessageNotification from 'widgets/notification/ui/MessageNotification';
import Notification from 'widgets/notification/ui/Notification';
import LoadingSpinner from 'shared/ui/LoadingSpinner/LoadingSpinner';
import ErrorMessage from 'shared/ui/ErrorMessage/ErrorMessage';
import ProductGallery from './ProductGallery';
import ProductInfo from './ProductInfo';
import ProductReviews from './ProductReviews';
import { http } from 'shared/api/httpClient';
import { API_URL } from 'shared/api/baseUrl';
import Configuration from 'shared/config/Configuration';    

function withParams(Component) {
    return function WrappedComponent(props) {
        const params = useParams();
        return <Component {...props} params={params} />;
    };
}

class ProductPage extends Component {
    static contextType = CartContext;

    constructor(props) {
        super(props);
        this.state = {
            product: null, loading: true, error: null, quantity: 1,
            selectedImage: null, isAddingToCart: false,
            notification: { show: false, message: '', productName: '' },
            reviews: [], reviewsLoading: false, averageRating: 0, totalReviews: 0,
            newReview: { rating: 5, comment: '' }, reviewSubmitting: false,
            editingReview: null, editComment: '', editRating: 5, editSubmitting: false,
            successMessage: null, errorMessage: null,
            attributes: [], attributesLoading: false,
        };
        this.abortController = null;
        this.productCache = new Map();
        this.notificationTimeout = null;
        this.messageTimeout = null;
    }

    componentDidMount() {
        const { id } = this.props.params || {};
        if (id) this.fetchProduct(id);
        else this.setState({ error: 'ID товара не указан', loading: false });
    }

    componentWillUnmount() {
        if (this.abortController) this.abortController.abort();
        if (this.notificationTimeout) clearTimeout(this.notificationTimeout);
        if (this.messageTimeout) clearTimeout(this.messageTimeout);
    }

    showSuccessMessage = (message) => {
        this.setState({ successMessage: message, errorMessage: null });
        if (this.messageTimeout) clearTimeout(this.messageTimeout);
        this.messageTimeout = setTimeout(() => this.setState({ successMessage: null }), 3000);
    };

    showErrorMessage = (message) => {
        this.setState({ errorMessage: message, successMessage: null });
        if (this.messageTimeout) clearTimeout(this.messageTimeout);
        this.messageTimeout = setTimeout(() => this.setState({ errorMessage: null }), 3000);
    };

    showNotification = (productName, quantity) => {
        if (this.notificationTimeout) clearTimeout(this.notificationTimeout);
        this.setState({
            notification: {
                show: true,
                message: 'Товар добавлен в корзину!',
                productName: `${productName} (${quantity} шт.)`
            }
        });
        this.notificationTimeout = setTimeout(() => {
            this.setState({ notification: { show: false, message: '', productName: '' } });
        }, 3000);
    };

    normalizeReviews = (reviewsArray) => {
        if (!Array.isArray(reviewsArray)) {
            reviewsArray = reviewsArray && typeof reviewsArray === 'object' ? Object.values(reviewsArray) : [];
        }
        return reviewsArray.map(review => ({
            id: review.Id ?? review.id,
            comment: review.Comment ?? review.comment,
            rating: review.Rating ?? review.rating,
            userName: review.UserName ?? review.userName ?? 'Аноним',
            userId: review.UserId ?? review.userId,
            createdAt: review.CreatedAt ?? review.createdAt,
            isApproved: review.IsApproved ?? review.isApproved
        }));
    };

    loadReviews = async (productId) => {
        this.setState({ reviewsLoading: true });
        try {
            const timestamp = Date.now();
            const response = await http(`${Configuration.Reviews.ByProduct}/${productId}?_=${timestamp}`, {
                headers: { 'Cache-Control': 'no-cache, no-store, must-revalidate', 'Pragma': 'no-cache' }
            });
            const data = await response.json();

            const reviewsArray = data.Reviews ?? data.reviews ?? data.items ?? [];

            this.setState({
                reviews: this.normalizeReviews(reviewsArray),
                averageRating: data.AverageRating ?? data.averageRating ?? 0,
                totalReviews: data.TotalReviews ?? data.totalReviews ?? 0,
                reviewsLoading: false
            });
        } catch (error) {
            console.error('Ошибка загрузки отзывов:', error);
            this.setState({ reviewsLoading: false });
        }
    };

    loadAttributes = async (productId) => {
        this.setState({ attributesLoading: true });
        try {
            const response = await http(`${Configuration.ProductAttributes.ByProduct}/${productId}`);
            const data = await response.json();
            this.setState({
                attributes: Array.isArray(data) ? data : [],
                attributesLoading: false
            });
        } catch (error) {
            console.error('Ошибка загрузки характеристик:', error);
            this.setState({ attributes: [], attributesLoading: false });
        }
    };

    startEditReview = (review) => {
        this.setState({ editingReview: review, editComment: review.comment, editRating: review.rating });
    };

    cancelEditReview = () => {
        this.setState({ editingReview: null, editComment: '', editRating: 5 });
    };

    updateReview = async () => {
        const { editingReview, editComment, editRating } = this.state;
        const token = localStorage.getItem('token');

        if (!editingReview) { this.showErrorMessage('Ошибка: нет отзыва'); return; }
        if (!editComment.trim()) { this.showErrorMessage('Введите текст отзыва'); return; }
        if (editComment.trim().length < 10) { this.showErrorMessage('Комментарий должен быть не короче 10 символов'); return; }
        if (editRating < 1 || editRating > 5) { this.showErrorMessage('Оценка 1-5'); return; }
        if (!token) { this.showErrorMessage('Войдите в систему'); return; }

        this.setState({ editSubmitting: true });
        try {
            await http.put(`${Configuration.Reviews.Update}/${editingReview.id}`, {
                Rating: editRating,
                Comment: editComment
            });
            this.showSuccessMessage('Отзыв обновлён!');
            this.cancelEditReview();
            await new Promise(r => setTimeout(r, 500));
            if (this.state.product?.Id) await this.loadReviews(this.state.product.Id);
            localStorage.setItem('needRefreshProfile', 'true');
        } catch (error) {
            console.error('Ошибка:', error);
            let message = 'Ошибка при обновлении отзыва';
            try {
                if (error.response) {
                    const errorData = await error.response.json();
                    message = errorData.error || errorData.message || message;
                }
            } catch (e) { /* ignore */ }
            this.showErrorMessage(message);
        } finally {
            this.setState({ editSubmitting: false });
        }
    };

    deleteReview = async (reviewId) => {
        const token = localStorage.getItem('token');
        const userId = localStorage.getItem('userId');
        if (!userId || !token) { this.showErrorMessage('Войдите в систему'); return; }
        if (!window.confirm('Удалить этот отзыв?')) return;

        try {
            await http.delete(`${Configuration.Reviews.Delete}/${reviewId}`);
            this.showSuccessMessage('Отзыв удалён!');
            await this.loadReviews(this.state.product.Id);
            localStorage.setItem('needRefreshProfile', 'true');
        } catch (error) {
            this.showErrorMessage('Ошибка удаления');
        }
    };

    submitReview = async () => {
        const { newReview, product } = this.state;
        const token = localStorage.getItem('token');
        const userId = localStorage.getItem('userId');
        if (!userId || !token) { this.showErrorMessage('Войдите в систему'); return; }
        if (!newReview.comment.trim()) { this.showErrorMessage('Введите текст отзыва'); return; }
        if (newReview.comment.trim().length < 10) { this.showErrorMessage('Комментарий должен быть не короче 10 символов'); return; }
        if (newReview.rating < 1 || newReview.rating > 5) { this.showErrorMessage('Оценка 1-5'); return; }

        this.setState({ reviewSubmitting: true });
        try {
            const response = await http.post(Configuration.Reviews.Create, {
                ProductId: product.Id,
                Rating: newReview.rating,
                Comment: newReview.comment
            });
            const result = await response.json();

            if (result.isSuccess || result.IsSuccess || result.success || result.Success) {
                this.showSuccessMessage(result.message || result.Message || 'Отзыв добавлен!');
                this.setState({ newReview: { rating: 5, comment: '' }, reviewSubmitting: false });
                await this.loadReviews(product.Id);
                localStorage.setItem('needRefreshProfile', 'true');
            } else {
                this.showErrorMessage(result.message || result.Message || 'Ошибка отправки');
                this.setState({ reviewSubmitting: false });
            }
        } catch (error) {
            console.error('Ошибка:', error);
            let message = 'Ошибка отправки';
            try {
                if (error.response) {
                    const errorData = await error.response.json();
                    message = errorData.error || errorData.message || message;
                }
            } catch (e) { /* ignore */ }
            this.showErrorMessage(message);
            this.setState({ reviewSubmitting: false });
        }
    };

    getCachedProduct = (productId) => {
        const cached = this.productCache.get(productId);
        if (cached && (Date.now() - cached.timestamp) < 300000) return cached.data;
        return null;
    };

    setCachedProduct = (productId, data) => {
        this.productCache.set(productId, { data, timestamp: Date.now() });
    };

    fetchProduct = async (productId) => {
        if (this.abortController) this.abortController.abort();
        this.abortController = new AbortController();

        try {
            this.setState({ loading: true });
            let product = this.getCachedProduct(productId);

            if (product) {
                this.setState({ product, loading: false, error: null });
                await this.loadReviews(productId);
                await this.loadAttributes(productId);
                return;
            }

            const response = await http(`${Configuration.Products.GetById}/${productId}`, {
                signal: this.abortController.signal
            });
            product = await response.json();
            this.setCachedProduct(productId, product);

            let mainImage = null;
            const images = product.Images || product.images || [];
            if (images.length > 0) {
                const mainImg = images.find(img => img.IsMain ?? img.isMain);
                mainImage = mainImg
                    ? (mainImg.ImageUrl ?? mainImg.imageUrl)
                    : (images[0].ImageUrl ?? images[0].imageUrl);
            }

            this.setState({ product, loading: false, error: null, selectedImage: mainImage });
            await this.loadReviews(productId);
            await this.loadAttributes(productId);
        } catch (error) {
            if (error.name === 'AbortError') return;
            this.setState({ error: error.message, loading: false });
        }
    };

    handleQuantityChange = (delta) => {
        this.setState(prev => ({ quantity: Math.max(1, prev.quantity + delta) }));
    };

    handleQuantityInput = (e) => {
        const value = parseInt(e.target.value);
        if (!isNaN(value) && value > 0) this.setState({ quantity: value });
    };

    handleAddToCart = async () => {
        const { product, quantity } = this.state;
        if (!product || !product.Id) { this.showErrorMessage('Товар не найден'); return; }
        this.setState({ isAddingToCart: true });
        const result = await this.context.addToCart(product.Id, quantity);
        this.setState({ isAddingToCart: false });
        if (result.success) this.showNotification(product.Name, quantity);
        else this.showErrorMessage('Ошибка добавления в корзину');
    };

    getMainImage = () => {
        const { product, selectedImage } = this.state;
        const images = product?.Images || product?.images || [];
        const fixUrl = (url) => {
            if (!url || url === 'string') return null;
            return url.startsWith('http') ? url : `${API_URL}${url.startsWith('/') ? '' : '/'}${url}`;
        };
        if (selectedImage) return fixUrl(selectedImage);
        if (images.length > 0) {
            const mainImg = images.find(img => img.IsMain ?? img.isMain);
            if (mainImg) return fixUrl(mainImg.ImageUrl ?? mainImg.imageUrl);
            return fixUrl(images[0].ImageUrl ?? images[0].imageUrl);
        }
        return null;
    };

    render() {
        const {
            product, loading, error, quantity, isAddingToCart, notification,
            reviews, reviewsLoading, averageRating, totalReviews,
            newReview, reviewSubmitting, editingReview, editComment, editRating, editSubmitting,
            successMessage, errorMessage,
            attributes, attributesLoading
        } = this.state;

        if (loading) {
            return <div className="product-page"><LoadingSpinner text="Загрузка товара..." /></div>;
        }

        if (error || !product) {
            return (
                <div className="product-page">
                    <ErrorMessage
                        message={error || 'Товар не найден'}
                        onRetry={() => this.props.navigate(-1)}
                    />
                </div>
            );
        }

        const images = product.Images || product.images || [];
        const mainImage = this.getMainImage();
        const currentUserId = localStorage.getItem('userId');
        const userRole = localStorage.getItem('userRole');

        return (
            <div className="product-page">
                <MessageNotification show={!!successMessage} message={successMessage} type="success" />
                <MessageNotification show={!!errorMessage} message={errorMessage} type="error" />
                <Notification show={notification.show} productName={notification.productName} />

                <Header navigate={this.props.navigate} />

                <main className="main-content">
                    <div className="container product-container">
                        <div className="product-header">
                            <h1 className="page-title">Карточка товара</h1>
                            <button className="back-button" onClick={() => this.props.navigate(-1)}>
                                Назад
                            </button>
                        </div>

                        <div className="product-main">
                            <ProductGallery
                                product={product}
                                mainImage={mainImage}
                                images={images}
                                selectedImage={this.state.selectedImage}
                                onImageSelect={(url) => this.setState({ selectedImage: url })}
                            />

                            <ProductInfo
                                product={product}
                                quantity={quantity}
                                isAddingToCart={isAddingToCart}
                                onQuantityIncrease={() => this.handleQuantityChange(1)}
                                onQuantityDecrease={() => this.handleQuantityChange(-1)}
                                onQuantityInput={this.handleQuantityInput}
                                onAddToCart={this.handleAddToCart}
                                attributes={attributes}
                                attributesLoading={attributesLoading}
                            />
                        </div>

                        <ProductReviews
                            reviews={reviews}
                            reviewsLoading={reviewsLoading}
                            averageRating={averageRating}
                            totalReviews={totalReviews}
                            newReview={newReview}
                            reviewSubmitting={reviewSubmitting}
                            editingReview={editingReview}
                            editComment={editComment}
                            editRating={editRating}
                            editSubmitting={editSubmitting}
                            currentUserId={currentUserId}
                            userRole={userRole}
                            onRatingChange={(rating) => this.setState({ newReview: { ...newReview, rating } })}
                            onCommentChange={(comment) => this.setState({ newReview: { ...newReview, comment } })}
                            onSubmitReview={this.submitReview}
                            onStartEdit={this.startEditReview}
                            onCancelEdit={this.cancelEditReview}
                            onSaveEdit={this.updateReview}
                            onChangeEditComment={(val) => this.setState({ editComment: val })}
                            onChangeEditRating={(val) => this.setState({ editRating: val })}
                            onDeleteReview={this.deleteReview}
                        />
                    </div>
                </main>
            </div>
        );
    }
}

export default withNavigate(withParams(ProductPage));