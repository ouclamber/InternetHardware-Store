import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import Computer from './Computer';

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
            { Id: 1, Name: 'Игровой компьютер MSI', Price: 189999, IsActive: true },
            { Id: 2, Name: 'Офисный компьютер HP', Price: 44999, IsActive: true },
            { Id: 3, Name: 'Домашний компьютер Dell', Price: 99999, IsActive: true }
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
        getBrandName: () => 'MSI'
    })
}));

describe('Computer — страница каталога', () => {
    beforeEach(() => {
        jest.clearAllMocks();
    });

    test('рендерит заголовок "Компьютеры"', () => {
        render(<Computer navigate={mockNavigate} />);
        expect(screen.getByText('Компьютеры')).toBeInTheDocument();
    });

    test('рендерит все товары', () => {
        render(<Computer navigate={mockNavigate} />);
        expect(screen.getByText('Игровой компьютер MSI')).toBeInTheDocument();
        expect(screen.getByText('Офисный компьютер HP')).toBeInTheDocument();
        expect(screen.getByText('Домашний компьютер Dell')).toBeInTheDocument();
    });

    test('рендерит сайдбар с фильтрами', () => {
        render(<Computer navigate={mockNavigate} />);
        expect(screen.getByText('Цена')).toBeInTheDocument();
        expect(screen.getByText('Сортировка')).toBeInTheDocument();
    });

    test('сортировка "Сначала дорогие" — MSI первый', () => {
        render(<Computer navigate={mockNavigate} />);
        fireEvent.change(screen.getByRole('combobox'), { target: { value: 'price_desc' } });

        const productNames = document.querySelectorAll('.product-name');
        expect(productNames[0]).toHaveTextContent('Игровой компьютер MSI');
    });

    test('фильтр "Цена от 100000" скрывает офисный ПК', () => {
        render(<Computer navigate={mockNavigate} />);
        fireEvent.change(screen.getByPlaceholderText('От'), { target: { value: '100000' } });

        expect(screen.queryByText('Офисный компьютер HP')).not.toBeInTheDocument();
        expect(screen.getByText('Игровой компьютер MSI')).toBeInTheDocument();
    });

    test('кнопка "Сбросить фильтры" работает', () => {
        render(<Computer navigate={mockNavigate} />);
        fireEvent.change(screen.getByPlaceholderText('От'), { target: { value: '100000' } });
        fireEvent.click(screen.getByText('Сбросить все фильтры'));

        expect(screen.getByText('Офисный компьютер HP')).toBeInTheDocument();
    });

    test('клик на "Назад" вызывает navigate(-1)', () => {
        render(<Computer navigate={mockNavigate} />);
        fireEvent.click(screen.getByTestId('back-button'));
        expect(mockNavigate).toHaveBeenCalledWith(-1);
    });
});