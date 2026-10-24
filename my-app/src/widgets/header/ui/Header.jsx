import React from 'react';
import CartIndicator from 'widgets/cart-indicator/ui/CartIndicator';
import { useWishlist } from 'entities/wishlist/model/WishlistContext';
import './Header.css';

const Header = ({ navigate, onSearch }) => {
    const { count: wishlistCount } = useWishlist();
    const handleProfile = () => navigate('/Profile');
    const handleWishlist = () => navigate('/Wishlist');

    const handleLogoClick = (e) => {
        e.preventDefault();
        navigate('/HomePage');
    };

    const handleSearchSubmit = (e) => {
        e.preventDefault();
        const input = e.target.querySelector('input[type="search"]');
        const query = input?.value;
        if (query && query.trim()) {
            if (onSearch) {
                onSearch(query.trim());
            } else {
                navigate(`/search?q=${encodeURIComponent(query.trim())}`);
            }
        }
    };

    return (
        <header className="header">
            <nav className="nav container">
                <a href="/" className="nav__logo" onClick={handleLogoClick}>
                    <div className="logo-text">
                        <span className="logo-primary">HardWare</span>
                        <span className="logo-secondary">Store</span>
                    </div>
                </a>

                <div className="search-full-width">
                    <div className="header-search-block">
                        <form onSubmit={handleSearchSubmit}>
                            <input
                                type="search"
                                placeholder="Поиск товаров..."
                                className="header-search-block__search"
                            />
                        </form>
                    </div>
                </div>

                <div className="header-icons">
                    <button className="icon-btn user-icon" onClick={handleProfile}>
                        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" aria-hidden="true">
                            <path d="M12 12C14.7614 12 17 9.76142 17 7C17 4.23858 14.7614 2 12 2C9.23858 2 7 4.23858 7 7C7 9.76142 9.23858 12 12 12Z" stroke="currentColor" strokeWidth="2"/>
                            <path d="M20 22C20 17.5817 16.4183 14 12 14C7.58172 14 4 17.5817 4 22" stroke="currentColor" strokeWidth="2"/>
                        </svg>
                    </button>
                    <button className="icon-btn wishlist-icon" onClick={handleWishlist} aria-label="Избранное" title="Избранное">
                        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" aria-hidden="true">
                            <path d="M12 21s-7.5-4.6-9.6-9.2C.9 8.3 3 4.5 6.7 4.5c2.1 0 3.6 1.2 4.3 2.4.7-1.2 2.2-2.4 4.3-2.4 3.7 0 5.8 3.8 4.3 7.3C19.5 16.4 12 21 12 21z" stroke="currentColor" strokeWidth="2"/>
                        </svg>
                        {wishlistCount > 0 && <span className="wishlist-badge" data-testid="wishlist-count">{wishlistCount}</span>}
                    </button>
                    <CartIndicator navigate={navigate} />
                </div>
            </nav>
        </header>
    );
};

export default Header;