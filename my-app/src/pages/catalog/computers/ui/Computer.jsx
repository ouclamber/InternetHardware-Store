import React, { useState, useMemo, useEffect } from 'react';
import './Computer.css';
import { withNavigate } from 'shared/lib/hoc/withNavigate';
import Header from 'widgets/header/ui/Header';
import Notification from 'widgets/notification/ui/Notification';
import ProductCard from 'entities/product/ui/ProductCard';
import LoadingSpinner from 'shared/ui/LoadingSpinner/LoadingSpinner';
import BackButton from 'shared/ui/BackButton/BackButton';
import CatalogLayout from 'shared/ui/CatalogLayout/CatalogLayout';
import Pagination from 'shared/ui/Pagination/Pagination';
import { useFetchProductsByCategory } from 'entities/product/model/useFetchProductsByCategory';
import { useProductHelpers } from 'entities/product/lib/useProductHelpers';
import { useAddToCart } from 'features/add-to-cart/model/useAddToCart';

const COMPUTER_BRANDS = {
    'msi': 'MSI', 'hp': 'HP', 'dell': 'Dell', 'asus': 'ASUS', 'lenovo': 'Lenovo'
};

const ITEMS_PER_PAGE = 8;

const pick = (obj, ...keys) => {
    for (const key of keys) {
        if (obj[key] !== undefined && obj[key] !== null) return obj[key];
    }
    return undefined;
};

const Computer = ({ navigate }) => {
    const { products, loading, error, fetchProducts, handleClearCache } = useFetchProductsByCategory(
        'Компьютеры',
        'computers_cache',
        'computers_etag'
    );
    const { getProductImage, getBrandName } = useProductHelpers();
    const { addToCart, notification } = useAddToCart();

    const [sortBy, setSortBy] = useState('relevance');
    const [minPrice, setMinPrice] = useState('');
    const [maxPrice, setMaxPrice] = useState('');
    const [currentPage, setCurrentPage] = useState(1);

    // Сброс страницы при изменении фильтров
    useEffect(() => {
        setCurrentPage(1);
    }, [sortBy, minPrice, maxPrice, products.length]);

    const getComputerType = (name) => {
        if (!name) return 'standard';
        const n = name.toLowerCase();
        if (n.includes('gaming') || n.includes('игровой')) return 'gaming';
        if (n.includes('office') || n.includes('офисный')) return 'office';
        if (n.includes('home') || n.includes('домашний')) return 'home';
        if (n.includes('budget') || n.includes('бюджетный')) return 'budget';
        return 'standard';
    };

    const filteredProducts = useMemo(() => {
        let filtered = [...products];

        if (minPrice) {
            filtered = filtered.filter(p => {
                const price = pick(p, 'Price', 'price') || 0;
                return price >= Number(minPrice);
            });
        }
        if (maxPrice) {
            filtered = filtered.filter(p => {
                const price = pick(p, 'Price', 'price') || 0;
                return price <= Number(maxPrice);
            });
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
    }, [products, minPrice, maxPrice, sortBy]);

    const totalPages = Math.ceil(filteredProducts.length / ITEMS_PER_PAGE);
    const paginatedProducts = filteredProducts.slice(
        (currentPage - 1) * ITEMS_PER_PAGE,
        currentPage * ITEMS_PER_PAGE
    );

    const handlePriceChange = (e) => {
        const { name, value } = e.target;
        if (name === 'minPrice') setMinPrice(value);
        if (name === 'maxPrice') setMaxPrice(value);
    };

    const handleSortChange = (e) => setSortBy(e.target.value);

    const handleClearFilters = () => {
        setSortBy('relevance');
        setMinPrice('');
        setMaxPrice('');
    };

    const hasActiveFilters = minPrice || maxPrice || sortBy !== 'relevance';

    const filters = (
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
                        onChange={handlePriceChange}
                    />
                    <span className="price-separator">—</span>
                    <input
                        type="number"
                        name="maxPrice"
                        className="price-input"
                        placeholder="До"
                        value={maxPrice}
                        onChange={handlePriceChange}
                    />
                </div>
            </div>

            <div className="filter-section">
                <h3 className="filter-title">Сортировка</h3>
                <select className="sort-select" value={sortBy} onChange={handleSortChange}>
                    <option value="relevance">По релевантности</option>
                    <option value="price_asc">Сначала дешевые</option>
                    <option value="price_desc">Сначала дорогие</option>
                    <option value="name_asc">По названию (А-Я)</option>
                </select>
            </div>

            {hasActiveFilters && (
                <button className="clear-filters-btn" onClick={handleClearFilters}>
                    Сбросить все фильтры
                </button>
            )}
        </>
    );

    return (
        <div className="computer-page">
            <Notification show={notification.show} productName={notification.productName} />
            <Header navigate={navigate} />

            <main className="main-content">
                <div className="container computer-container">
                    <BackButton onClick={() => navigate(-1)} />

                    <CatalogLayout
                        title="Компьютеры"
                        filters={filters}
                        pagination={
                            <Pagination
                                currentPage={currentPage}
                                totalPages={totalPages}
                                onPageChange={setCurrentPage}
                            />
                        }
                    >
                        {loading && <LoadingSpinner text="Загрузка..." />}

                        {error && !loading && (
                            <div className="error-message">
                                <p>{error}</p>
                                <button onClick={fetchProducts} className="retry-btn">Повторить</button>
                                {handleClearCache && (
                                    <button onClick={handleClearCache} className="retry-btn" style={{ marginLeft: 10 }}>
                                        Очистить кэш
                                    </button>
                                )}
                            </div>
                        )}

                        {!loading && !error && paginatedProducts.length === 0 && (
                            <div className="no-products">
                                <p>Товары не найдены</p>
                            </div>
                        )}

                        {!loading && !error && paginatedProducts.length > 0 && (
                            <div className="products-grid">
                                {paginatedProducts.map((product, index) => (
                                    <ProductCard
                                        key={pick(product, 'Id', 'id') ?? index}
                                        product={product}
                                        onClick={(id) => navigate(`/product/${id}`)}
                                        onAddToCart={addToCart}
                                        getBrandName={(p) => getBrandName(p, COMPUTER_BRANDS)}
                                        getProductImage={getProductImage}
                                        getProductType={(p) => getComputerType(pick(p, 'Name', 'name'))}
                                        placeholderText="Компьютер"
                                    />
                                ))}
                            </div>
                        )}
                    </CatalogLayout>
                </div>
            </main>
        </div>
    );
};

export default withNavigate(Computer);