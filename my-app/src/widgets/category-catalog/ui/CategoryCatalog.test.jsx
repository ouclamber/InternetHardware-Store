import React from 'react';
import { render, screen, fireEvent, act } from '@testing-library/react';
import CategoryCatalog, { PRICE_DEBOUNCE_MS } from './CategoryCatalog';
import { useCategoryProducts } from 'entities/product/model/useCategoryProducts';

jest.mock('widgets/header/ui/Header', () => () => <div data-testid="header" />);
jest.mock('widgets/notification/ui/Notification', () => ({ show, message }) =>
    show ? <div data-testid="notification">{message}</div> : null);
jest.mock('shared/ui/BackButton/BackButton', () => ({ onClick }) => (
    <button data-testid="back-button" onClick={onClick}>Назад</button>
));
jest.mock('features/toggle-wishlist/ui/WishlistButton', () => ({ productId }) => (
    <button data-testid={`wish-${productId}`}>♥</button>
));

jest.mock('entities/product/model/useCategoryProducts', () => ({
    useCategoryProducts: jest.fn()
}));

const mockAddToCart = jest.fn();
jest.mock('features/add-to-cart/model/useAddToCart', () => ({
    useAddToCart: () => ({
        addToCart: mockAddToCart,
        notification: { show: false, productName: '', message: '', type: 'success' }
    })
}));

jest.mock('entities/product/lib/useProductHelpers', () => ({
    useProductHelpers: () => ({
        getProductImage: () => null,
        getBrandName: (p, hints) => {
            const name = (p.Name || '').toLowerCase();
            const key = Object.keys(hints).find(k => name.includes(k));
            return key ? hints[key] : 'Не указан';
        }
    })
}));

const PRODUCTS = [
    { Id: 1, Name: 'Apple iPhone 15 Pro', Price: 139999, IsActive: true, StockQuantity: 5 },
    { Id: 2, Name: 'Samsung Galaxy S24', Price: 129999, IsActive: true, StockQuantity: 0 }
];

const hookResult = (overrides = {}) => ({
    products: PRODUCTS,
    totalCount: 14,
    totalPages: 3,
    loading: false,
    error: null,
    reload: jest.fn(),
    ...overrides
});

const lastHookArgs = () => useCategoryProducts.mock.calls[useCategoryProducts.mock.calls.length - 1];

const renderCatalog = (props = {}) => {
    const navigate = jest.fn();
    render(
        <CategoryCatalog
            navigate={navigate}
            title="Смартфоны"
            categoryName="Смартфоны"
            brandHints={{ apple: 'Apple', samsung: 'Samsung' }}
            typeRules={[[['iphone'], 'iphone']]}
            placeholderText="Смартфон"
            pageClassName="smartphones-page"
            containerClassName="smartphones-container"
            {...props}
        />
    );
    return navigate;
};

describe('CategoryCatalog — общая страница категории', () => {
    beforeEach(() => {
        jest.clearAllMocks();
        useCategoryProducts.mockReturnValue(hookResult());
    });

    afterEach(() => {
        jest.useRealTimers();
    });

    test('рендерит заголовок, товары и количество найденных', () => {
        renderCatalog();

        expect(screen.getByText('Смартфоны')).toBeInTheDocument();
        expect(screen.getByText('Apple iPhone 15 Pro')).toBeInTheDocument();
        expect(screen.getByText('Samsung Galaxy S24')).toBeInTheDocument();
        expect(screen.getByText('Найдено товаров: 14')).toBeInTheDocument();
    });

    test('применяет CSS-классы страницы и контейнера', () => {
        renderCatalog();

        expect(document.querySelector('.smartphones-page')).not.toBeNull();
        expect(document.querySelector('.container.smartphones-container')).not.toBeNull();
    });

    test('первоначальный запрос: категория, 1-я страница, без фильтров', () => {
        renderCatalog();

        const [categoryName, params] = lastHookArgs();
        expect(categoryName).toBe('Смартфоны');
        expect(params).toEqual({
            sort: 'relevance', minPrice: '', maxPrice: '', inStockOnly: false, page: 1, pageSize: 6
        });
    });

    test('сортировка передаётся на сервер', () => {
        renderCatalog();

        fireEvent.change(screen.getByLabelText('Сортировка'), { target: { value: 'price_desc' } });

        expect(lastHookArgs()[1].sort).toBe('price_desc');
    });

    test('цена отправляется на сервер только после паузы ввода (debounce)', () => {
        jest.useFakeTimers();
        renderCatalog();

        fireEvent.change(screen.getByPlaceholderText('От'), { target: { value: '100000' } });
        expect(lastHookArgs()[1].minPrice).toBe('');

        act(() => { jest.advanceTimersByTime(PRICE_DEBOUNCE_MS); });
        expect(lastHookArgs()[1].minPrice).toBe('100000');
    });

    test('некорректный диапазон цены: подсказка и верхняя граница не отправляется', () => {
        jest.useFakeTimers();
        renderCatalog();

        fireEvent.change(screen.getByPlaceholderText('От'), { target: { value: '5000' } });
        fireEvent.change(screen.getByPlaceholderText('До'), { target: { value: '100' } });
        act(() => { jest.advanceTimersByTime(PRICE_DEBOUNCE_MS); });

        expect(screen.getByText('Минимальная цена больше максимальной')).toBeInTheDocument();
        expect(lastHookArgs()[1].minPrice).toBe('5000');
        expect(lastHookArgs()[1].maxPrice).toBe('');
    });

    test('чекбокс «Только в наличии»', () => {
        renderCatalog();

        fireEvent.click(screen.getByLabelText('Только в наличии'));

        expect(lastHookArgs()[1].inStockOnly).toBe(true);
    });

    test('пагинация меняет страницу, смена фильтра возвращает на первую', () => {
        renderCatalog();

        fireEvent.click(screen.getByText('2'));
        expect(lastHookArgs()[1].page).toBe(2);

        fireEvent.change(screen.getByLabelText('Сортировка'), { target: { value: 'name_asc' } });
        expect(lastHookArgs()[1].page).toBe(1);
    });

    test('кнопка «Сбросить все фильтры» появляется и сбрасывает фильтры', () => {
        renderCatalog();
        expect(screen.queryByText('Сбросить все фильтры')).not.toBeInTheDocument();

        fireEvent.click(screen.getByLabelText('Только в наличии'));
        fireEvent.change(screen.getByLabelText('Сортировка'), { target: { value: 'price_asc' } });
        fireEvent.click(screen.getByText('Сбросить все фильтры'));

        expect(lastHookArgs()[1]).toMatchObject({ sort: 'relevance', inStockOnly: false });
        expect(screen.queryByText('Сбросить все фильтры')).not.toBeInTheDocument();
    });

    test('товар без остатка — кнопка «В корзину» неактивна', () => {
        renderCatalog();

        const cards = document.querySelectorAll('.product-card');
        expect(cards[0].querySelector('.add-to-cart-btn')).not.toBeDisabled();
        expect(cards[1].querySelector('.add-to-cart-btn')).toBeDisabled();
        expect(screen.getByText('Нет в наличии')).toBeInTheDocument();
    });

    test('у каждой карточки есть кнопка «В избранное»', () => {
        renderCatalog();

        expect(screen.getByTestId('wish-1')).toBeInTheDocument();
        expect(screen.getByTestId('wish-2')).toBeInTheDocument();
    });

    test('бренд определяется по подсказкам страницы', () => {
        renderCatalog();

        expect(screen.getAllByText('Apple').length).toBeGreaterThan(0);
    });

    test('клик по карточке ведёт на страницу товара', () => {
        const navigate = renderCatalog();

        fireEvent.click(screen.getByText('Apple iPhone 15 Pro'));

        expect(navigate).toHaveBeenCalledWith('/product/1');
    });

    test('«В корзину» вызывает addToCart', () => {
        renderCatalog();

        fireEvent.click(document.querySelectorAll('.add-to-cart-btn')[0]);

        expect(mockAddToCart).toHaveBeenCalledWith(1, 1, expect.anything(), 'Apple iPhone 15 Pro');
    });

    test('«Назад» вызывает navigate(-1)', () => {
        const navigate = renderCatalog();

        fireEvent.click(screen.getByTestId('back-button'));

        expect(navigate).toHaveBeenCalledWith(-1);
    });

    test('состояние загрузки', () => {
        useCategoryProducts.mockReturnValue(hookResult({ loading: true, products: [] }));
        renderCatalog();

        expect(screen.getByText('Загрузка...')).toBeInTheDocument();
        expect(screen.queryByText('Товары не найдены')).not.toBeInTheDocument();
    });

    test('ошибка и повтор загрузки', () => {
        const reload = jest.fn();
        useCategoryProducts.mockReturnValue(hookResult({ error: 'Сервер недоступен', products: [], reload }));
        renderCatalog();

        expect(screen.getByText('Сервер недоступен')).toBeInTheDocument();
        fireEvent.click(screen.getByText('Повторить'));
        expect(reload).toHaveBeenCalled();
    });

    test('пустой результат', () => {
        useCategoryProducts.mockReturnValue(hookResult({ products: [], totalCount: 0, totalPages: 0 }));
        renderCatalog();

        expect(screen.getByText('Товары не найдены')).toBeInTheDocument();
        expect(screen.queryByText(/Найдено товаров/)).not.toBeInTheDocument();
    });
});
