import React, { Component } from 'react';
import './Cart.css';
import { withNavigate } from 'shared/lib/hoc/withNavigate';
import { CartContext } from 'entities/cart/model/CartContext';
import Header from 'widgets/header/ui/Header';
import MessageNotification from 'widgets/notification/ui/MessageNotification';
import BackButton from 'shared/ui/BackButton/BackButton';
import LoadingSpinner from 'shared/ui/LoadingSpinner/LoadingSpinner';
import ErrorMessage from 'shared/ui/ErrorMessage/ErrorMessage';
import QuantityControl from 'entities/product/ui/QuantityControl';
import { formatPrice } from 'shared/lib/utils/formatPrice';
import { http } from 'shared/api/httpClient';
import { API_URL } from 'shared/api/baseUrl';
import Configuration from 'shared/config/Configuration';

class CartPage extends Component {
    static contextType = CartContext;

    constructor(props) {
        super(props);
        this.state = {
            cartItems: [],
            loading: true,
            error: null,
            summary: { totalQuantity: 0, totalPrice: 0, itemCount: 0 },
            updatingItemId: null,
            notification: { show: false, message: '', type: 'success' }
        };
        this.abortController = null;
        this.notificationTimeout = null;
    }

    componentDidMount() {
        const contextItems = this.context?.cartItems || [];
        const formattedItems = this.formatItems(contextItems);
        this.setState({ cartItems: formattedItems, loading: false });
        this.calculateSummary(formattedItems);

        if (contextItems.length === 0 && this.context?.refreshCart) {
            this.context.refreshCart().then(() => {
                const freshItems = this.context?.cartItems || [];
                const formatted = this.formatItems(freshItems);
                this.setState({ cartItems: formatted, loading: false });
                this.calculateSummary(formatted);
            });
        }
    }

    componentDidUpdate(prevProps, prevState) {
        // Синхронизируем с контекстом, если он изменился
        const contextItems = this.context?.cartItems || [];
        if (contextItems !== prevState.cartItems && this.state.cartItems.length !== contextItems.length) {
            const formattedItems = this.formatItems(contextItems);
            this.setState({ cartItems: formattedItems });
            this.calculateSummary(formattedItems);
        }
    }

    componentWillUnmount() {
        if (this.abortController) this.abortController.abort();
        if (this.notificationTimeout) clearTimeout(this.notificationTimeout);
    }

    showNotification = (message, type = 'success') => {
        if (this.notificationTimeout) clearTimeout(this.notificationTimeout);
        this.setState({ notification: { show: true, message, type } });
        this.notificationTimeout = setTimeout(() => {
            this.setState({ notification: { show: false, message: '', type: 'success' } });
        }, 3000);
    };

    formatItems = (cartItems) => {
        return cartItems.map(item => ({
            id: item.id,
            productId: item.productId,
            quantity: item.quantity,
            product: item.product ? {
                id: item.product.id,
                name: item.product.name,
                price: item.product.price,
                brand: item.product.brand,
                images: item.product.images || []
            } : null
        }));
    };

    calculateSummary = (items) => {
        const totalQuantity = items.reduce((sum, item) => sum + (item.quantity || 0), 0);
        const totalPrice = items.reduce((sum, item) => sum + ((item.product?.price || 0) * (item.quantity || 0)), 0);
        const itemCount = items.length;

        this.setState({ summary: { totalQuantity, totalPrice, itemCount } });
    };

    updateQuantity = async (productId, newQuantity) => {
        if (newQuantity < 1) {
            this.removeItem(productId);
            return;
        }
        this.setState({ updatingItemId: productId });

        try {
            await http.put(`${Configuration.Cart.UpdateItem}/${productId}`, { quantity: newQuantity });

            const updatedItems = this.state.cartItems.map(item =>
                item.productId === productId ? { ...item, quantity: newQuantity } : item
            );
            this.setState({ cartItems: updatedItems });
            this.calculateSummary(updatedItems);

            if (this.context?.updateCartCount) {
                await this.context.updateCartCount();
            }
        } catch (error) {
            console.error('Ошибка обновления количества:', error);
            this.showNotification('Не удалось обновить количество', 'error');
        } finally {
            this.setState({ updatingItemId: null });
        }
    };

    removeItem = async (productId) => {
        this.setState({ updatingItemId: productId });
        try {
            await http.delete(`${Configuration.Cart.RemoveItem}/${productId}`);

            const updatedItems = this.state.cartItems.filter(item => item.productId !== productId);
            this.setState({ cartItems: updatedItems });
            this.calculateSummary(updatedItems);

            if (this.context?.updateCartCount) {
                await this.context.updateCartCount();
            }
        } catch (error) {
            console.error('Ошибка удаления товара:', error);
            this.showNotification('Не удалось удалить товар', 'error');
        } finally {
            this.setState({ updatingItemId: null });
        }
    };

    clearCart = async () => {
        if (!window.confirm('Вы уверены, что хотите очистить корзину?')) return;

        this.setState({ loading: true });
        const result = await this.context.clearCart();

        if (result.success) {
            this.setState({ cartItems: [], loading: false });
            this.calculateSummary([]);
            this.showNotification('Корзина успешно очищена!', 'success');
        } else {
            this.setState({ loading: false });
            this.showNotification('Ошибка при очистке корзины', 'error');
        }
    };

    handleCheckout = () => {
        const { cartItems } = this.state;
        if (!cartItems || cartItems.length === 0) {
            this.showNotification('Корзина пуста', 'error');
            return;
        }

        const cartItemsCopy = cartItems.map(item => ({
            id: item.id,
            productId: item.productId,
            quantity: Number(item.quantity),
            product: item.product ? {
                id: item.product.id,
                name: item.product.name,
                price: Number(item.product.price),
                images: Array.isArray(item.product.images) ? item.product.images : []
            } : null
        }));

        localStorage.setItem('needRefreshProfile', 'true');
        this.props.navigate('/checkout', { state: { cartItems: cartItemsCopy } });
    };

    getImageUrl = (item) => {
        let imageUrl = item.product?.images?.[0]?.imageUrl;
        if (imageUrl && !imageUrl.startsWith('http')) {
            imageUrl = `${API_URL}${imageUrl.startsWith('/') ? '' : '/'}${imageUrl}`;
        }
        return imageUrl;
    };

    render() {
        const { cartItems, loading, error, summary, updatingItemId, notification } = this.state;

        if (loading) {
            return (
                <div className="cart-page">
                    <LoadingSpinner text="Загрузка корзины..." />
                </div>
            );
        }

        if (error) {
            return (
                <div className="cart-page">
                    <ErrorMessage message={error} onRetry={this.context?.refreshCart} />
                </div>
            );
        }

        return (
            <div className="cart-page">
                <MessageNotification show={notification.show} message={notification.message} type={notification.type} />

                <Header navigate={this.props.navigate} />

                <main className="main-content">
                    <div className="container cart-container">
                        <div className="cart-header">
                            <h1 className="page-title">Корзина</h1>
                            <BackButton onClick={() => this.props.navigate(-1)} />
                        </div>

                        {cartItems.length === 0 ? (
                            <div className="empty-cart">
                                <h2>Ваша корзина пуста</h2>
                                <p>Добавьте товары из каталога, чтобы оформить заказ</p>
                                <button className="continue-shopping" onClick={() => this.props.navigate('/HomePage')}>
                                    Продолжить покупки
                                </button>
                            </div>
                        ) : (
                            <div className="cart-content">
                                <div className="cart-items">
                                    {cartItems.map((item) => {
                                        const product = item.product;
                                        const price = product?.price || 0;
                                        const quantity = item.quantity || 1;
                                        const totalPrice = price * quantity;
                                        const imageUrl = this.getImageUrl(item);

                                        return (
                                            <div key={item.productId} className="cart-item">
                                                <div className="cart-item-image">
                                                    {imageUrl ? (
                                                        <img
                                                            src={imageUrl}
                                                            alt={product?.name}
                                                            onError={(e) => { e.target.onerror = null; e.target.style.display = 'none'; }}
                                                        />
                                                    ) : (
                                                        <div className="no-image-placeholder">📷</div>
                                                    )}
                                                </div>

                                                <div className="cart-item-info">
                                                    <h3 className="cart-item-title">{product?.name || 'Товар'}</h3>
                                                    <div className="cart-item-brand">
                                                        Бренд: {product?.brand?.name || 'Не указан'}
                                                    </div>
                                                    <div className="cart-item-price">{formatPrice(price)} / шт.</div>
                                                </div>

                                                <div className="cart-item-quantity">
                                                    <QuantityControl
                                                        quantity={quantity}
                                                        onIncrease={() => this.updateQuantity(item.productId, quantity + 1)}
                                                        onDecrease={() => this.updateQuantity(item.productId, quantity - 1)}
                                                        disabled={updatingItemId === item.productId}
                                                        min={1}
                                                        showInput={false}
                                                    />
                                                </div>

                                                <div className="cart-item-total">{formatPrice(totalPrice)}</div>

                                            </div>
                                        );
                                    })}
                                </div>

                                <div className="cart-summary">
                                    <h3 className="summary-title">Итого</h3>
                                    <div className="summary-row">
                                        <span>Товары ({summary.itemCount} шт.)</span>
                                        <span>{formatPrice(summary.totalPrice)}</span>
                                    </div>
                                    <div className="summary-row delivery">
                                        <span>Доставка</span>
                                        <span className="free">Бесплатно</span>
                                    </div>
                                    <div className="summary-total">
                                        <span>К оплате</span>
                                        <span className="total-price">{formatPrice(summary.totalPrice)}</span>
                                    </div>

                                    <div className="cart-actions">
                                        <button className="clear-cart-btn" onClick={this.clearCart}>
                                            Очистить корзину
                                        </button>
                                        <button className="checkout-btn" onClick={this.handleCheckout}>
                                            Оформить заказ
                                        </button>
                                    </div>
                                </div>
                            </div>
                        )}
                    </div>
                </main>
            </div>
        );
    }
}

export default withNavigate(CartPage);