import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import Phone from './Phone';

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
            { Id: 1, Name: 'Apple iPhone 15 Pro', Price: 139999, IsActive: true },
            { Id: 2, Name: 'Samsung Galaxy S24', Price: 129999, IsActive: true },
            { Id: 3, Name: 'Xiaomi 14 Pro', Price: 89999, IsActive: true }
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
        getBrandName: () => 'Apple'
    })
}));

describe('Phone — страница каталога', () => {
    beforeEach(() => {
        jest.clearAllMocks();
    });

    test('рендерит заголовок "Смартфоны"', () => {
        render(<Phone navigate={mockNavigate} />);
        expect(screen.getByText('Смартфоны')).toBeInTheDocument();
    });

    test('рендерит все товары', () => {
        render(<Phone navigate={mockNavigate} />);
        expect(screen.getByText('Apple iPhone 15 Pro')).toBeInTheDocument();
        expect(screen.getByText('Samsung Galaxy S24')).toBeInTheDocument();
        expect(screen.getByText('Xiaomi 14 Pro')).toBeInTheDocument();
    });

    test('рендерит сайдбар с фильтрами', () => {
        render(<Phone navigate={mockNavigate} />);
        expect(screen.getByText('Цена')).toBeInTheDocument();
        expect(screen.getByText('Сортировка')).toBeInTheDocument();
    });

    test('сортировка "Сначала дорогие"', () => {
        render(<Phone navigate={mockNavigate} />);
        fireEvent.change(screen.getByRole('combobox'), { target: { value: 'price_desc' } });

        const productNames = document.querySelectorAll('.product-name');
        expect(productNames[0]).toHaveTextContent('Apple iPhone 15 Pro');
    });

    test('фильтр по цене скрывает дешёвые', () => {
        render(<Phone navigate={mockNavigate} />);
        fireEvent.change(screen.getByPlaceholderText('От'), { target: { value: '100000' } });

        expect(screen.queryByText('Xiaomi 14 Pro')).not.toBeInTheDocument();
    });

    test('сброс фильтров восстанавливает список', () => {
        render(<Phone navigate={mockNavigate} />);
        fireEvent.change(screen.getByPlaceholderText('От'), { target: { value: '100000' } });
        fireEvent.click(screen.getByText('Сбросить все фильтры'));

        expect(screen.getByText('Xiaomi 14 Pro')).toBeInTheDocument();
    });

    test('клик "Назад" → navigate(-1)', () => {
        render(<Phone navigate={mockNavigate} />);
        fireEvent.click(screen.getByTestId('back-button'));
        expect(mockNavigate).toHaveBeenCalledWith(-1);
    });
});