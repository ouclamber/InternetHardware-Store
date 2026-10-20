import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import ProductCard from './ProductCard';

describe('ProductCard', () => {
    const mockProduct = {
        id: 1,
        name: 'ASUS VivoBook 15',
        price: 54999,
        isActive: true,
        description: 'Мощный ноутбук для работы и учёбы',
        brand: { name: 'ASUS' }
    };

    const mockProps = {
        product: mockProduct,
        onClick: jest.fn(),
        onAddToCart: jest.fn(),
        getBrandName: () => 'ASUS',
        getProductImage: () => 'https://example.com/image.jpg',
        getProductType: () => 'standard'
    };

    beforeEach(() => {
        jest.clearAllMocks();
    });

    test('рендерит название товара', () => {
        render(<ProductCard {...mockProps} />);
        expect(screen.getByText('ASUS VivoBook 15')).toBeInTheDocument();
    });

    test('рендерит цену в правильном формате', () => {
        render(<ProductCard {...mockProps} />);
        expect(screen.getByText('54 999 ₽')).toBeInTheDocument();
    });

    test('рендерит бренд', () => {
        render(<ProductCard {...mockProps} />);
        const brandElements = screen.getAllByText('ASUS');
        expect(brandElements.length).toBeGreaterThan(0);
    });

    test('показывает "В наличии" для активного товара', () => {
        render(<ProductCard {...mockProps} />);
        expect(screen.getByText('В наличии')).toBeInTheDocument();
    });

    test('показывает "Нет в наличии" для неактивного товара', () => {
        const inactiveProduct = { ...mockProduct, isActive: false };
        render(<ProductCard {...mockProps} product={inactiveProduct} />);

        const elements = screen.getAllByText('Нет в наличии');
        expect(elements.length).toBeGreaterThan(0);
    });

    test('кнопка "В корзину" активна для активного товара', () => {
        render(<ProductCard {...mockProps} />);
        const button = screen.getByRole('button');
        expect(button).not.toBeDisabled();
        expect(button).toHaveTextContent('В корзину');
    });

    test('кнопка "В корзину" отключена для неактивного товара', () => {
        const inactiveProduct = { ...mockProduct, isActive: false };
        render(<ProductCard {...mockProps} product={inactiveProduct} />);
        const button = screen.getByRole('button');
        expect(button).toBeDisabled();
    });

    test('вызывает onClick при клике на карточку', () => {
        render(<ProductCard {...mockProps} />);
        const card = screen.getByText('ASUS VivoBook 15').closest('.product-card');
        fireEvent.click(card);
        expect(mockProps.onClick).toHaveBeenCalledWith(mockProduct.id);
    });

    test('вызывает onAddToCart при клике на кнопку', () => {
        render(<ProductCard {...mockProps} />);
        const button = screen.getByRole('button');
        fireEvent.click(button);
        expect(mockProps.onAddToCart).toHaveBeenCalled();
    });

    test('использует placeholderText если нет названия', () => {
        const noNameProduct = { ...mockProduct, name: '' };
        render(<ProductCard {...mockProps} product={noNameProduct} placeholderText="Ноутбук" />);
        expect(screen.getByText('Ноутбук')).toBeInTheDocument();
    });

    test('показывает описание если оно есть', () => {
        render(<ProductCard {...mockProps} />);
        const description = screen.getByText(/Мощный ноутбук/);
        expect(description).toBeInTheDocument();
    });

    test('показывает placeholder если нет изображения', () => {
        const propsWithoutImage = {
            ...mockProps,
            getProductImage: () => null
        };
        render(<ProductCard {...propsWithoutImage} />);
        expect(screen.getByText('📷')).toBeInTheDocument();
    });
});