import React, { Component } from 'react';
import './Home.css';
import { withNavigate } from 'shared/lib/hoc/withNavigate';
import Header from 'widgets/header/ui/Header';
import CategoryCard from 'entities/category/ui/CategoryCard';
import LoadingSpinner from 'shared/ui/LoadingSpinner/LoadingSpinner';
import ErrorMessage from 'shared/ui/ErrorMessage/ErrorMessage';
import { http } from 'shared/api/httpClient';
import { API_URL } from 'shared/api/baseUrl';
import Configuration from 'shared/config/Configuration';

const CATEGORY_CACHE = { data: null, timestamp: null, expiry: 5 * 60 * 1000 };
const STORAGE_KEY = 'categories_cache';
const ETAG_KEY = 'categories_etag';

const CATEGORY_ORDER = ['Телевизоры', 'Ноутбуки', 'Компьютеры', 'Смартфоны', 'Колонки'];

class HomePage extends Component {
    constructor(props) {
        super(props);
        this.state = {
            categories: [],
            categoriesLoading: true,
            error: null,
            fromCache: false
        };
        this.abortController = null;
    }

    loadFromCache = () => {
        try {
            if (CATEGORY_CACHE.data && CATEGORY_CACHE.timestamp &&
                (Date.now() - CATEGORY_CACHE.timestamp < CATEGORY_CACHE.expiry)) {
                this.setState({ categories: CATEGORY_CACHE.data, categoriesLoading: false, fromCache: true });
                return true;
            }
            const cached = localStorage.getItem(STORAGE_KEY);
            if (cached) {
                const { data, timestamp } = JSON.parse(cached);
                if (Date.now() - timestamp < CATEGORY_CACHE.expiry) {
                    CATEGORY_CACHE.data = data;
                    CATEGORY_CACHE.timestamp = timestamp;
                    this.setState({ categories: data, categoriesLoading: false, fromCache: true });
                    return true;
                }
            }
        } catch (error) {
            console.warn('Ошибка загрузки кэша:', error);
        }
        return false;
    };

    saveToCache = (data) => {
        try {
            CATEGORY_CACHE.data = data;
            CATEGORY_CACHE.timestamp = Date.now();
            localStorage.setItem(STORAGE_KEY, JSON.stringify({ data, timestamp: Date.now() }));
        } catch (error) {
            console.warn('Ошибка сохранения кэша:', error);
        }
    };

    componentDidMount() {
        const fromCache = this.loadFromCache();
        if (!fromCache) {
            this.fetchCategoriesFromAPI();
        }
    }

    componentWillUnmount() {
        if (this.abortController) this.abortController.abort();
    }

    fetchCategoriesFromAPI = async () => {
        if (this.abortController) this.abortController.abort();
        this.abortController = new AbortController();

        try {
            this.setState({ categoriesLoading: true, error: null });

            const [categoriesResponse, productsResponse] = await Promise.all([
                http(Configuration.Categories.GetAll, { signal: this.abortController.signal }),
                http(Configuration.Products.GetAll, { signal: this.abortController.signal })
            ]);

            const categoriesData = await categoriesResponse.json();
            const productsData = await productsResponse.json();

            // Считаем товары по категориям (categoryId с маленькой буквы — наш API отдаёт camelCase для DTO)
            const countsByCategory = {};
            productsData.forEach(product => {
                const catId = product.categoryId ?? product.CategoryId;
                if (catId) {
                    countsByCategory[catId] = (countsByCategory[catId] || 0) + 1;
                }
            });

            // Форматируем категории (наш API отдаёт PascalCase)
            const formattedCategories = categoriesData
                .map(category => {
                    const id = category.Id ?? category.id;
                    const name = category.Name ?? category.name ?? '';
                    const description = category.Description ?? category.description;
                    const imageUrl = category.ImageUrl ?? category.imageUrl;

                    return {
                        id,
                        name,
                        description: description || `${name} в нашем магазине`,
                        productCount: countsByCategory[id] || 0,
                        imageUrl: imageUrl && imageUrl !== 'string' ? imageUrl : null
                    };
                })
                .filter(c => c.id !== undefined && c.name)
                .sort((a, b) => {
                    const indexA = CATEGORY_ORDER.findIndex(n =>
                        a.name.toLowerCase().includes(n.toLowerCase()));
                    const indexB = CATEGORY_ORDER.findIndex(n =>
                        b.name.toLowerCase().includes(n.toLowerCase()));
                    return (indexA === -1 ? 999 : indexA) - (indexB === -1 ? 999 : indexB);
                });

            this.saveToCache(formattedCategories);
            this.setState({ categories: formattedCategories, categoriesLoading: false });
        } catch (error) {
            if (error.name === 'AbortError') return;

            if (CATEGORY_CACHE.data?.length > 0) {
                this.setState({
                    categories: CATEGORY_CACHE.data,
                    categoriesLoading: false,
                    error: 'Используются кэшированные данные'
                });
            } else {
                this.setState({
                    error: `Не удалось загрузить категории: ${error.message}`,
                    categoriesLoading: false
                });
            }
        }
    };

    getCategoryImage = (category) => {
        let imageUrl = category.imageUrl;
        if (imageUrl && !imageUrl.startsWith('http')) {
            imageUrl = `${API_URL}${imageUrl.startsWith('/') ? '' : '/'}${imageUrl}`;
        }
        return imageUrl;
    };

    handleCategoryClick = (categoryId, categoryName) => {
        const name = categoryName.toLowerCase();
        const { navigate } = this.props;
        if (name.includes('телевизор')) navigate('/Television');
        else if (name.includes('ноутбук')) navigate('/Labtop');
        else if (name.includes('компьютер')) navigate('/Computer');
        else if (name.includes('колонки')) navigate('/Speaker');
        else if (name.includes('смартфоны')) navigate('/Phone');
        else alert(`Категория "${categoryName}" пока в разработке`);
    };

    handleClearCache = () => {
        localStorage.removeItem(STORAGE_KEY);
        localStorage.removeItem(ETAG_KEY);
        CATEGORY_CACHE.data = null;
        CATEGORY_CACHE.timestamp = null;
        this.setState({ categories: [], categoriesLoading: true });
        this.fetchCategoriesFromAPI();
    };

    render() {
        const { categories, categoriesLoading, error } = this.state;

        return (
            <div className="home-page">
                <Header navigate={this.props.navigate} />

                <main className="main-content">
                    <div className="container">
                        <div className="catalog-header">
                            <h1 className="catalog-title">Каталог</h1>
                            <p className="catalog-subtitle">Выберите интересующую вас категорию товаров</p>
                        </div>

                        {categoriesLoading && <LoadingSpinner text="Загрузка категорий..." />}

                        {error && !categoriesLoading && (
                            <ErrorMessage
                                message={error}
                                onRetry={this.fetchCategoriesFromAPI}
                                onClearCache={this.handleClearCache}
                            />
                        )}

                        {!categoriesLoading && !error && (
                            <div className="categories-grid">
                                {categories.length > 0 ? (
                                    categories.map((category, index) => (
                                        <CategoryCard
                                            key={category.id}
                                            category={category}
                                            index={index}
                                            onClick={this.handleCategoryClick}
                                            getCategoryImage={this.getCategoryImage}
                                        />
                                    ))
                                ) : (
                                    <div className="no-categories">
                                        <p>Категории не найдены</p>
                                        <button onClick={this.fetchCategoriesFromAPI} className="retry-btn">
                                            Обновить
                                        </button>
                                    </div>
                                )}
                            </div>
                        )}
                    </div>
                </main>
            </div>
        );
    }
}

export default withNavigate(HomePage);