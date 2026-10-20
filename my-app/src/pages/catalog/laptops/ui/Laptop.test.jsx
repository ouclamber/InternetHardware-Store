import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import Laptop from './Laptop';

const mockNavigate = jest.fn();

jest.mock('shared/lib/hoc/withNavigate', () => ({
    withNavigate: (Component) => (props) => (
        <Component {...props} navigate={mockNavigate} />
    )
}));

jest.mock('widgets/header/ui/Header', () => () => <div data-testid="header" />);
jest.mock('widgets/notification/ui/Notification', () => () => null);
jest.mock('shared/ui/BackButton/BackButton', () => ({ onClick }) => (
    <button data-testid="back-button" onClick={onClick}>Назад</button>
));

jest.mock('entities/product/model/useFetchProductsByCategory', () => ({
    useFetchProductsByCategory: () => ({
        products: [
            { Id: 1, Name: 'ASUS ROG Strix G16', Price: 129999, IsActive: true },
            { Id: 2, Name: 'MacBook Air M3', Price: 149999, IsActive: true },
            { Id: 3, Name: 'Lenovo IdeaPad 3', Price: 44999, IsActive: true }
        ],
        loading: false,
        error: null,
        fetchProducts: jest.fn(),
        handleClearCache: jest.fn()
    })
}));

jest.mock('features/add-to-cart/model/useAddToCart', () => ({
    useAddToCart: () => ({
        addToCart: jest.fn(),
        notification: { show: false, productName: '' }
    })
}));

jest.mock('entities/product/lib/useProductHelpers', () => ({
    useProductHelpers: () => ({
        getProductImage: () => null,
        getBrandName: () => 'ASUS'
    })
}));

describe('Laptop — страница каталога', () => {
    beforeEach(() => {
        jest.clearAllMocks();
    });

    test('рендерит заголовок "Ноутбуки"', () => {
        render(<Laptop navigate={mockNavigate} />);
        expect(screen.getByText('Ноутбуки')).toBeInTheDocument();
    });

    test('рендерит кнопку "Назад"', () => {
        render(<Laptop navigate={mockNavigate} />);
        expect(screen.getByTestId('back-button')).toBeInTheDocument();
    });

    test('рендерит все товары', () => {
        render(<Laptop navigate={mockNavigate} />);
        expect(screen.getByText('ASUS ROG Strix G16')).toBeInTheDocument();
        expect(screen.getByText('MacBook Air M3')).toBeInTheDocument();
        expect(screen.getByText('Lenovo IdeaPad 3')).toBeInTheDocument();
    });

    test('рендерит сайдбар с фильтрами', () => {
        render(<Laptop navigate={mockNavigate} />);
        expect(screen.getByText('Цена')).toBeInTheDocument();
        expect(screen.getByText('Сортировка')).toBeInTheDocument();
    });

    test('сортировка "Сначала дорогие" — MacBook первый', () => {
        render(<Laptop navigate={mockNavigate} />);
        const select = screen.getByRole('combobox');
        fireEvent.change(select, { target: { value: 'price_desc' } });

        const productNames = document.querySelectorAll('.product-name');
        expect(productNames[0]).toHaveTextContent('MacBook Air M3');
    });

    test('сортировка "Сначала дешёвые" — Lenovo первый', () => {
        render(<Laptop navigate={mockNavigate} />);
        const select = screen.getByRole('combobox');
        fireEvent.change(select, { target: { value: 'price_asc' } });

        const productNames = document.querySelectorAll('.product-name');
        expect(productNames[0]).toHaveTextContent('Lenovo IdeaPad 3');
    });

    test('фильтр "Цена от" скрывает дешёвые товары', () => {
        render(<Laptop navigate={mockNavigate} />);
        const minInput = screen.getByPlaceholderText('От');
        fireEvent.change(minInput, { target: { value: '100000' } });

        expect(screen.queryByText('Lenovo IdeaPad 3')).not.toBeInTheDocument();
        expect(screen.getByText('MacBook Air M3')).toBeInTheDocument();
    });

    test('фильтр "Цена до" скрывает дорогие товары', () => {
        render(<Laptop navigate={mockNavigate} />);
        const maxInput = screen.getByPlaceholderText('До');
        fireEvent.change(maxInput, { target: { value: '50000' } });

        expect(screen.queryByText('MacBook Air M3')).not.toBeInTheDocument();
        expect(screen.getByText('Lenovo IdeaPad 3')).toBeInTheDocument();
    });

    test('кнопка "Сбросить фильтры" появляется при активных фильтрах', () => {
        render(<Laptop navigate={mockNavigate} />);
        const minInput = screen.getByPlaceholderText('От');
        fireEvent.change(minInput, { target: { value: '100000' } });

        expect(screen.getByText('Сбросить все фильтры')).toBeInTheDocument();
    });

    test('кнопка "Сбросить фильтры" скрывается после сброса', () => {
        render(<Laptop navigate={mockNavigate} />);
        const minInput = screen.getByPlaceholderText('От');
        fireEvent.change(minInput, { target: { value: '100000' } });

        const resetBtn = screen.getByText('Сбросить все фильтры');
        fireEvent.click(resetBtn);

        expect(screen.queryByText('Сбросить все фильтры')).not.toBeInTheDocument();
        expect(screen.getByText('Lenovo IdeaPad 3')).toBeInTheDocument();
    });

    test('клик на "Назад" вызывает navigate(-1)', () => {
        render(<Laptop navigate={mockNavigate} />);
        fireEvent.click(screen.getByTestId('back-button'));
        expect(mockNavigate).toHaveBeenCalledWith(-1);
    });
});