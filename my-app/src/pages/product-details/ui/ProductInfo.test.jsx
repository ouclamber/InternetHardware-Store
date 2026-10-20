import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import ProductInfo from './ProductInfo';

const mockProduct = {
    Id: 1,
    Name: 'Apple MacBook Air M2',
    Price: 119999,
    IsActive: true
};

const mockAttributes = [
    { Id: 1, AttributeName: 'Процессор', Group: 'Основные', Unit: '', Value: 'Apple M2' },
    { Id: 2, AttributeName: 'RAM', Group: 'Основные', Unit: 'ГБ', Value: '8' },
    { Id: 3, AttributeName: 'Диагональ', Group: 'Экран', Unit: 'дюймы', Value: '13.6' }
];

const defaultProps = {
    product: mockProduct,
    quantity: 1,
    isAddingToCart: false,
    onQuantityIncrease: jest.fn(),
    onQuantityDecrease: jest.fn(),
    onQuantityInput: jest.fn(),
    onAddToCart: jest.fn(),
    attributes: [],
    attributesLoading: false
};

describe('ProductInfo', () => {
    beforeEach(() => {
        jest.clearAllMocks();
    });

    test('рендерит название товара', () => {
        render(<ProductInfo {...defaultProps} />);
        expect(screen.getByText('Apple MacBook Air M2')).toBeInTheDocument();
    });

    test('рендерит цену', () => {
        render(<ProductInfo {...defaultProps} />);
        const priceEl = document.querySelector('.product-price');
        expect(priceEl).toHaveTextContent(/119/);
    });

    test('рендерит "В наличии" для активного товара', () => {
        render(<ProductInfo {...defaultProps} />);
        expect(screen.getByText('В наличии')).toBeInTheDocument();
    });

    test('рендерит блок "Характеристики"', () => {
        render(<ProductInfo {...defaultProps} />);
        expect(screen.getByText('Характеристики:')).toBeInTheDocument();
    });

    test('показывает "Характеристики отсутствуют" при пустом массиве', () => {
        render(<ProductInfo {...defaultProps} attributes={[]} />);
        expect(screen.getByText('Характеристики отсутствуют')).toBeInTheDocument();
    });

    test('показывает "Загрузка..." при attributesLoading', () => {
        render(<ProductInfo {...defaultProps} attributesLoading={true} />);
        expect(screen.getByText('Загрузка...')).toBeInTheDocument();
    });

    test('рендерит характеристики с группировкой', () => {
        render(<ProductInfo {...defaultProps} attributes={mockAttributes} />);
        expect(screen.getByText('Основные')).toBeInTheDocument();
        expect(screen.getByText('Экран')).toBeInTheDocument();
        expect(screen.getByText('Процессор')).toBeInTheDocument();
        expect(screen.getByText('Apple M2')).toBeInTheDocument();
    });

    test('рендерит единицы измерения', () => {
        render(<ProductInfo {...defaultProps} attributes={mockAttributes} />);
        expect(screen.getByText(/8 ГБ/)).toBeInTheDocument();
    });

    test('отображает quantity', () => {
        render(<ProductInfo {...defaultProps} quantity={3} />);
        const input = screen.getByDisplayValue('3');
        expect(input).toBeInTheDocument();
    });

    test('клик на + вызывает onQuantityIncrease', () => {
        render(<ProductInfo {...defaultProps} />);
        const plusBtn = screen.getByText('+');
        fireEvent.click(plusBtn);
        expect(defaultProps.onQuantityIncrease).toHaveBeenCalled();
    });

    test('клик на − вызывает onQuantityDecrease', () => {
        render(<ProductInfo {...defaultProps} quantity={2} />);
        const minusBtn = screen.getByText('−');
        fireEvent.click(minusBtn);
        expect(defaultProps.onQuantityDecrease).toHaveBeenCalled();
    });

    test('клик на "В корзину" вызывает onAddToCart', () => {
        render(<ProductInfo {...defaultProps} />);
        const cartBtn = screen.getByText('В корзину');
        fireEvent.click(cartBtn);
        expect(defaultProps.onAddToCart).toHaveBeenCalled();
    });

    test('кнопка "В корзину" disabled при isAddingToCart', () => {
        render(<ProductInfo {...defaultProps} isAddingToCart={true} />);
        const cartBtn = screen.getByText('Добавление...');
        expect(cartBtn).toBeDisabled();
    });

    test('кнопка "−" disabled при quantity=1', () => {
        render(<ProductInfo {...defaultProps} quantity={1} />);
        const minusBtn = screen.getByText('−');
        expect(minusBtn).toBeDisabled();
    });

    test('показывает "Итого"', () => {
        render(<ProductInfo {...defaultProps} />);
        expect(screen.getByText(/Итого:/)).toBeInTheDocument();
    });
});