import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import Television from './Television';

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
            { Id: 1, Name: 'Samsung QE55Q80C', Price: 109999, IsActive: true },
            { Id: 2, Name: 'LG OLED55C3', Price: 149999, IsActive: true },
            { Id: 3, Name: 'Xiaomi TV Q2 55', Price: 59999, IsActive: true }
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
        getBrandName: () => 'Samsung'
    })
}));

describe('Television — страница каталога', () => {
    beforeEach(() => {
        jest.clearAllMocks();
    });

    test('рендерит заголовок "Телевизоры"', () => {
        render(<Television navigate={mockNavigate} />);
        expect(screen.getByText('Телевизоры')).toBeInTheDocument();
    });

    test('рендерит все товары', () => {
        render(<Television navigate={mockNavigate} />);
        expect(screen.getByText('Samsung QE55Q80C')).toBeInTheDocument();
        expect(screen.getByText('LG OLED55C3')).toBeInTheDocument();
        expect(screen.getByText('Xiaomi TV Q2 55')).toBeInTheDocument();
    });

    test('рендерит сайдбар с фильтрами', () => {
        render(<Television navigate={mockNavigate} />);
        expect(screen.getByText('Цена')).toBeInTheDocument();
        expect(screen.getByText('Сортировка')).toBeInTheDocument();
    });

    test('сортировка "Сначала дорогие" — LG первый', () => {
        render(<Television navigate={mockNavigate} />);
        fireEvent.change(screen.getByRole('combobox'), { target: { value: 'price_desc' } });

        const productNames = document.querySelectorAll('.product-name');
        expect(productNames[0]).toHaveTextContent('LG OLED55C3');
    });

    test('фильтр "Цена от 100000" скрывает Xiaomi', () => {
        render(<Television navigate={mockNavigate} />);
        fireEvent.change(screen.getByPlaceholderText('От'), { target: { value: '100000' } });

        expect(screen.queryByText('Xiaomi TV Q2 55')).not.toBeInTheDocument();
    });

    test('сброс фильтров восстанавливает список', () => {
        render(<Television navigate={mockNavigate} />);
        fireEvent.change(screen.getByPlaceholderText('От'), { target: { value: '100000' } });
        fireEvent.click(screen.getByText('Сбросить все фильтры'));

        expect(screen.getByText('Xiaomi TV Q2 55')).toBeInTheDocument();
    });

    test('клик "Назад" → navigate(-1)', () => {
        render(<Television navigate={mockNavigate} />);
        fireEvent.click(screen.getByTestId('back-button'));
        expect(mockNavigate).toHaveBeenCalledWith(-1);
    });
});