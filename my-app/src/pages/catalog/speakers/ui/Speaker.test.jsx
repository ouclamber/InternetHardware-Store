import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import Speaker from './Speaker';

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
            { Id: 1, Name: 'JBL Charge 5', Price: 17999, IsActive: true },
            { Id: 2, Name: 'JBL Flip 6', Price: 12999, IsActive: true },
            { Id: 3, Name: 'Sony SRS-XB43', Price: 19999, IsActive: true }
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
        getBrandName: () => 'JBL'
    })
}));

describe('Speaker — страница каталога', () => {
    beforeEach(() => {
        jest.clearAllMocks();
    });

    test('рендерит заголовок "Колонки"', () => {
        render(<Speaker navigate={mockNavigate} />);
        expect(screen.getByText('Колонки')).toBeInTheDocument();
    });

    test('рендерит все товары', () => {
        render(<Speaker navigate={mockNavigate} />);
        expect(screen.getByText('JBL Charge 5')).toBeInTheDocument();
        expect(screen.getByText('JBL Flip 6')).toBeInTheDocument();
        expect(screen.getByText('Sony SRS-XB43')).toBeInTheDocument();
    });

    test('рендерит сайдбар с фильтрами', () => {
        render(<Speaker navigate={mockNavigate} />);
        expect(screen.getByText('Цена')).toBeInTheDocument();
        expect(screen.getByText('Сортировка')).toBeInTheDocument();
    });

    test('сортировка "Сначала дорогие" — Sony первый', () => {
        render(<Speaker navigate={mockNavigate} />);
        fireEvent.change(screen.getByRole('combobox'), { target: { value: 'price_desc' } });

        const productNames = document.querySelectorAll('.product-name');
        expect(productNames[0]).toHaveTextContent('Sony SRS-XB43');
    });

    test('фильтр "Цена до 15000" скрывает дорогие', () => {
        render(<Speaker navigate={mockNavigate} />);
        fireEvent.change(screen.getByPlaceholderText('До'), { target: { value: '15000' } });

        expect(screen.queryByText('Sony SRS-XB43')).not.toBeInTheDocument();
        expect(screen.getByText('JBL Flip 6')).toBeInTheDocument();
    });

    test('сброс фильтров восстанавливает список', () => {
        render(<Speaker navigate={mockNavigate} />);
        fireEvent.change(screen.getByPlaceholderText('До'), { target: { value: '15000' } });
        fireEvent.click(screen.getByText('Сбросить все фильтры'));

        expect(screen.getByText('Sony SRS-XB43')).toBeInTheDocument();
    });

    test('клик "Назад" → navigate(-1)', () => {
        render(<Speaker navigate={mockNavigate} />);
        fireEvent.click(screen.getByTestId('back-button'));
        expect(mockNavigate).toHaveBeenCalledWith(-1);
    });
});