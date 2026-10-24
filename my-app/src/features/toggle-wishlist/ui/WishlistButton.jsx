import React, { useState } from 'react';
import { useWishlist } from 'entities/wishlist/model/WishlistContext';
import './WishlistButton.css';

/**
 * Кнопка «В избранное» (сердечко). Клик не всплывает до карточки товара.
 */
const WishlistButton = ({ productId, size = 'small', onResult }) => {
    const { isInWishlist, toggle, isAuthenticated } = useWishlist();
    const [busy, setBusy] = useState(false);

    if (productId === undefined || productId === null) return null;

    const active = isInWishlist(productId);
    const label = active ? 'Убрать из избранного' : 'Добавить в избранное';

    const handleClick = async (e) => {
        e.stopPropagation();
        e.preventDefault();
        if (busy) return;

        setBusy(true);
        const result = await toggle(productId);
        setBusy(false);
        if (onResult) onResult(result);
    };

    return (
        <button
            type="button"
            className={`wishlist-btn wishlist-btn--${size} ${active ? 'wishlist-btn--active' : ''}`}
            onClick={handleClick}
            aria-pressed={active}
            aria-label={label}
            title={isAuthenticated ? label : 'Войдите, чтобы добавлять в избранное'}
            disabled={busy}
        >
            <svg viewBox="0 0 24 24" width="20" height="20" aria-hidden="true">
                <path
                    d="M12 21s-7.5-4.6-9.6-9.2C.9 8.3 3 4.5 6.7 4.5c2.1 0 3.6 1.2 4.3 2.4.7-1.2 2.2-2.4 4.3-2.4 3.7 0 5.8 3.8 4.3 7.3C19.5 16.4 12 21 12 21z"
                    fill={active ? 'currentColor' : 'none'}
                    stroke="currentColor"
                    strokeWidth="2"
                />
            </svg>
            {size === 'large' && <span className="wishlist-btn__text">{active ? 'В избранном' : 'В избранное'}</span>}
        </button>
    );
};

export default WishlistButton;
