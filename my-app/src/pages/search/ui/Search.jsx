import React, { Component } from 'react';
import './Search.css';
import { withNavigate } from 'shared/lib/hoc/withNavigate';
import { CartContext } from 'entities/cart/model/CartContext';
import Header from 'widgets/header/ui/Header';
import Notification from 'widgets/notification/ui/Notification';
import ProductCard from 'entities/product/ui/ProductCard';
import LoadingSpinner from 'shared/ui/LoadingSpinner/LoadingSpinner';
import BackButton from 'shared/ui/BackButton/BackButton';
import CatalogLayout from 'shared/ui/CatalogLayout/CatalogLayout';
import Pagination from 'shared/ui/Pagination/Pagination';
import { http } from 'shared/api/httpClient';
import { API_URL } from 'shared/api/baseUrl';
import Configuration from 'shared/config/Configuration';

const pick = (obj, ...keys) => {
    for (const key of keys) {
        if (obj[key] !== undefined && obj[key] !== null) return obj[key];
    }
    return undefined;
};

const ITEMS_PER_PAGE = 6;

class SearchPage extends Component {
    static contextType = CartContext;

    constructor(props) {
        super(props);
        this.state = {
            query: '',
            products: [],
            allProducts: [],
            loading: false,
            error: null,
            searchPerformed: false,
            minPrice: '',
            maxPrice: '',
            sortBy: 'relevance',
            currentPage: 1,
            notification: { show: false, message: '', productName: '' }
        };
        this.searchTimeout = null;
        this.abortController = null;
        this.notificationTimeout = null;
    }

    componentDidMount() {
        const urlParams = new URLSearchParams(window.location.search);
        const query = urlParams.get('q') || '';

        if (query) {
            this.setState({ query }, () => this.performSearch());
        }
    }

    componentWillUnmount() {
        if (this.searchTimeout) clearTimeout(this.searchTimeout);
        if (this.abortController) this.abortController.abort();
        if (this.notificationTimeout) clearTimeout(this.notificationTimeout);
    }

    showNotification = (productName) => {
        if (this.notificationTimeout) clearTimeout(this.notificationTimeout);
        this.setState({ notification: { show: true, message: 'Товар добавлен в корзину!', productName } });
        this.notificationTimeout = setTimeout(() => {
            this.setState({ notification: { show: false, message: '', productName: '' } });
        }, 3000);
    };

    handleInputChange = (e) => {
        const query = e.target.value;
        this.setState({ query });
        if (this.searchTimeout) clearTimeout(this.searchTimeout);
        this.searchTimeout = setTimeout(() => {
            if (query.length >= 2 || query.length === 0) this.performSearch();
        }, 500);
    };

    performSearch = async () => {
        const { query } = this.state;

        if (!query.trim()) {
            this.setState({
                products: [],
                allProducts: [],
                searchPerformed: false,
                loading: false,
                currentPage: 1
            });
            return;
        }

        if (this.abortController) this.abortController.abort();
        this.abortController = new AbortController();

        this.setState({ loading: true, error: null, searchPerformed: true, currentPage: 1 });

        try {
            const response = await http(
                `${Configuration.Products.Search}?q=${encodeURIComponent(query)}`,
                { signal: this.abortController.signal }
            );
            const products = await response.json();

            this.setState({ allProducts: products, loading: false });

            const urlParams = new URLSearchParams();
            if (query) urlParams.set('q', query);
            window.history.pushState({}, '', `${window.location.pathname}?${urlParams.toString()}`);
        } catch (error) {
            if (error.name === 'AbortError') return;
            this.setState({ error: 'Ошибка при выполнении поиска', loading: false, products: [] });
        }
    };

    getFilteredProducts = () => {
        const { allProducts, minPrice, maxPrice, sortBy } = this.state;

        let filtered = [...allProducts];

        if (minPrice) {
            filtered = filtered.filter(p => (pick(p, 'Price', 'price') || 0) >= Number(minPrice));
        }
        if (maxPrice) {
            filtered = filtered.filter(p => (pick(p, 'Price', 'price') || 0) <= Number(maxPrice));
        }

        if (sortBy === 'price_asc') {
            filtered.sort((a, b) => (pick(a, 'Price', 'price') || 0) - (pick(b, 'Price', 'price') || 0));
        } else if (sortBy === 'price_desc') {
            filtered.sort((a, b) => (pick(b, 'Price', 'price') || 0) - (pick(a, 'Price', 'price') || 0));
        } else if (sortBy === 'name_asc') {
            filtered.sort((a, b) => {
                const na = pick(a, 'Name', 'name') || '';
                const nb = pick(b, 'Name', 'name') || '';
                return na.localeCompare(nb);
            });
        }

        return filtered;
    };

    handlePriceChange = (e) => {
        const { name, value } = e.target;
        this.setState({ [name]: value, currentPage: 1 });
    };

    handleSortChange = (e) => {
        this.setState({ sortBy: e.target.value, currentPage: 1 });
    };

    handleClearFilters = () => {
        this.setState({ minPrice: '', maxPrice: '', sortBy: 'relevance', currentPage: 1 });
    };

    handlePageChange = (page) => {
        this.setState({ currentPage: page });
        window.scrollTo({ top: 0, behavior: 'smooth' });
    };

    handleProductClick = (productId) => this.props.navigate(`/product/${productId}`);

    handleAddToCart = async (productId, quantity, event, productName) => {
        if (event) event.stopPropagation();
        const result = await this.context.addToCart(productId, quantity);
        if (result.success) this.showNotification(productName);
    };

    renderFilters = () => {
        const { minPrice, maxPrice, sortBy } = this.state;
        const hasActiveFilters = minPrice || maxPrice || sortBy !== 'relevance';

        return (
            <>
                <div className="filter-section">
                    <h3 className="filter-title">Цена</h3>
                    <div className="price-range">
                        <input
                            type="number"
                            name="minPrice"
                            className="price-input"
                            placeholder="От"
                            value={minPrice}
                            onChange={this.handlePriceChange}
                        />
                        <span className="price-separator">—</span>
                        <input
                            type="number"
                            name="maxPrice"
                            className="price-input"
                            placeholder="До"
                            value={maxPrice}
                            onChange={this.handlePriceChange}
                        />
                    </div>
                </div>

                <div className="filter-section">
                    <h3 className="filter-title">Сортировка</h3>
                    <select className="sort-select" value={sortBy} onChange={this.handleSortChange}>
                        <option value="relevance">По релевантности</option>
                        <option value="price_asc">Сначала дешевые</option>
                        <option value="price_desc">Сначала дорогие</option>
                        <option value="name_asc">По названию (А-Я)</option>
                    </select>
                </div>

                {hasActiveFilters && (
                    <button className="clear-filters-btn" onClick={this.handleClearFilters}>
                        Сбросить все фильтры
                    </button>
                )}
            </>
        );
    };

    render() {
        const { query, loading, error, searchPerformed, currentPage, notification } = this.state;

        const filteredProducts = this.getFilteredProducts();
        const totalPages = Math.ceil(filteredProducts.length / ITEMS_PER_PAGE);
        const paginatedProducts = filteredProducts.slice(
            (currentPage - 1) * ITEMS_PER_PAGE,
            currentPage * ITEMS_PER_PAGE
        );

        const pageTitle = searchPerformed
            ? (query ? `Результаты поиска: "${query}"` : 'Все товары')
            : 'Поиск товаров';

        return (
            <div className="search-page">
                <Notification show={notification.show} productName={notification.productName} />

                <Header
                    navigate={this.props.navigate}
                    onSearch={(q) => this.setState({ query: q }, () => this.performSearch())}
                />

                <main className="main-content">
                    <div className="container search-container">
                        <BackButton onClick={() => this.props.navigate('/HomePage')} text="На главную" />

                        <CatalogLayout
                            title={pageTitle}
                            filters={this.renderFilters()}
                            pagination={
                                <Pagination
                                    currentPage={currentPage}
                                    totalPages={totalPages}
                                    onPageChange={this.handlePageChange}
                                />
                            }
                        >
                            {searchPerformed && !loading && !error && (
                                <p className="search-results-count">
                                    Найдено {filteredProducts.length} товаров
                                </p>
                            )}

                            {loading && <LoadingSpinner text="Поиск..." />}

                            {error && !loading && (
                                <div className="error-message">
                                    <p>{error}</p>
                                    <button onClick={this.performSearch} className="retry-btn">
                                        Повторить
                                    </button>
                                </div>
                            )}

                            {!loading && !error && searchPerformed && paginatedProducts.length === 0 && (
                                <div className="no-results">
                                    <h3>Ничего не найдено</h3>
                                    <p>Попробуйте изменить поисковый запрос или фильтры</p>
                                </div>
                            )}

                            {!loading && !error && paginatedProducts.length > 0 && (
                                <div className="products-grid">
                                    {paginatedProducts.map((product, index) => (
                                        <ProductCard
                                            key={pick(product, 'Id', 'id') ?? index}
                                            product={product}
                                            onClick={this.handleProductClick}
                                            onAddToCart={this.handleAddToCart}
                                            getBrandName={(p) => pick(p, 'BrandName', 'brandName') || 'Не указан'}
                                            getProductImage={(p) => {
                                                let url = pick(p, 'ImageUrl', 'imageUrl', 'MainImage', 'mainImage');
                                                if (!url) {
                                                    const imgs = p.Images ?? p.images;
                                                    if (Array.isArray(imgs) && imgs.length > 0) {
                                                        const main = imgs.find(i => i.IsMain ?? i.isMain) ?? imgs[0];
                                                        url = pick(main, 'ImageUrl', 'imageUrl');
                                                    }
                                                }
                                                if (!url || url === 'string') return null;
                                                return url.startsWith('http')
                                                    ? url
                                                    : `${API_URL}${url.startsWith('/') ? '' : '/'}${url}`;
                                            }}
                                            getProductType={() => 'standard'}
                                            placeholderText="Товар"
                                        />
                                    ))}
                                </div>
                            )}
                        </CatalogLayout>
                    </div>
                </main>
            </div>
        );
    }
}

export default withNavigate(SearchPage);