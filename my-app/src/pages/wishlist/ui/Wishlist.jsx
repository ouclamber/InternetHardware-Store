import React, { useCallback, useEffect, useState } from 'react';
import { withNavigate } from 'shared/lib/hoc/withNavigate';
import { http } from 'shared/api/httpClient';
import Configuration from 'shared/config/Configuration';
import Header from 'widgets/header/ui/Header';
import Notification from 'widgets/notification/ui/Notification';
import ProductCard from 'entities/product/ui/ProductCard';
import { useProductHelpers } from 'entities/product/lib/useProductHelpers';
import { userStorage } from 'entities/user/lib/userStorage';
import { useWishlist } from 'entities/wishlist/model/WishlistContext';
import { useAddToCart } from 'features/add-to-cart/model/useAddToCart';
import WishlistButton from 'features/toggle-wishlist/ui/WishlistButton';
import LoadingSpinner from 'shared/ui/LoadingSpinner/LoadingSpinner';
import BackButton from 'shared/ui/BackButton/BackButton';
import './Wishlist.css';

const pick = (obj, ...keys) => {
    for (const key of keys) {
        if (obj && obj[key] !== undefined && obj[key] !== null) return obj[key];
    }
    return undefined;
};

/**
 * Страница «Избранное»: отложенные товары пользователя.
 */
const Wishlist = ({ navigate }) => {
    const [items, setItems] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    const { getProductImage, getBrandName } = useProductHelpers();
    const { addToCart, notification } = useAddToCart();
    const { refresh } = useWishlist();

    const isAuthenticated = userStorage.isAuthenticated();

    const load = useCallback(async () => {
        setLoading(true);
        setError(null);
        try {
            const response = await http(Configuration.Wishlist.Get);
            const data = await response.json();
            setItems(Array.isArray(data) ? data : []);
        } catch (err) {
            setError(err.message || 'Не удалось загрузить избранное');
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => {
        if (isAuthenticated) load();
        else setLoading(false);
    }, [isAuthenticated, load]);

    const handleToggled = (productId) => (result) => {
        if (result?.success && result.added === false) {
            setItems(prev => prev.filter(i => pick(i, 'ProductId', 'productId') !== productId));
            refresh();
        }
    };

    const renderBody = () => {
        if (!isAuthenticated) {
            return (
                <div className="wishlist-empty">
                    <p>Войдите, чтобы сохранять понравившиеся товары.</p>
                    <button className="wishlist-cta" onClick={() => navigate('/SignIn')}>Войти</button>
                </div>
            );
        }

        if (loading) return <LoadingSpinner text="Загрузка избранного..." />;

        if (error) {
            return (
                <div className="error-message">
                    <p>{error}</p>
                    <button className="retry-btn" onClick={load}>Повторить</button>
                </div>
            );
        }

        if (items.length === 0) {
            return (
                <div className="wishlist-empty">
                    <p>В избранном пока пусто.</p>
                    <p className="wishlist-empty__hint">Нажмите на сердечко на карточке товара, чтобы отложить его.</p>
                    <button className="wishlist-cta" onClick={() => navigate('/HomePage')}>Перейти в каталог</button>
                </div>
            );
        }

        return (
            <div className="products-grid wishlist-grid">
                {items.map(item => {
                    const productId = pick(item, 'ProductId', 'productId');
                    const product = pick(item, 'Product', 'product') || {};
                    return (
                        <ProductCard
                            key={productId}
                            product={product}
                            onClick={(id) => navigate(`/product/${id}`)}
                            onAddToCart={addToCart}
                            getBrandName={(p) => getBrandName(p)}
                            getProductImage={getProductImage}
                            actions={<WishlistButton productId={productId} onResult={handleToggled(productId)} />}
                        />
                    );
                })}
            </div>
        );
    };

    return (
        <div className="wishlist-page">
            <Notification
                show={notification.show}
                productName={notification.productName}
                message={notification.message}
                type={notification.type}
            />
            <Header navigate={navigate} />
            <main className="main-content">
                <div className="container wishlist-container">
                    <BackButton onClick={() => navigate(-1)} />
                    <h1 className="page-title">
                        Избранное{items.length > 0 ? <span className="wishlist-count"> ({items.length})</span> : null}
                    </h1>
                    {renderBody()}
                </div>
            </main>
        </div>
    );
};

export default withNavigate(Wishlist);
