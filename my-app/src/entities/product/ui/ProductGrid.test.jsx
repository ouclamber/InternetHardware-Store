import React from 'react';
import { render, screen } from '@testing-library/react';
import ProductGrid from './ProductGrid';

const mockProducts = [
    { Id: 1, Name: 'ASUS ROG', Price: 129999 },
    { Id: 2, Name: 'MacBook Air', Price: 149999 }
];

const mockProps = {
    onProductClick: jest.fn(),
    onAddToCart: jest.fn(),
    getBrandName: () => 'ASUS',
    getProductImage: () => 'https://example.com/img.jpg',
    getProductType: () => 'standard'
};

describe('ProductGrid', () => {
    test('рендерит loading spinner', () => {
        render(<ProductGrid {...mockProps} products={[]} loading={true} />);
        expect(screen.getByText(/Загрузка/i)).toBeInTheDocument();
    });

    test('рендерит error message', () => {
        render(<ProductGrid {...mockProps} products={[]} error="Ошибка загрузки" />);
        expect(screen.getByText('Ошибка загрузки')).toBeInTheDocument();
    });

    test('рендерит "Нет товаров" для пустого массива', () => {
        render(<ProductGrid {...mockProps} products={[]} loading={false} />);
        expect(screen.getByText(/Нет товаров|не найдены/i)).toBeInTheDocument();
    });

    test('рендерит карточки товаров', () => {
        render(<ProductGrid {...mockProps} products={mockProducts} loading={false} />);
        expect(screen.getByText('ASUS ROG')).toBeInTheDocument();
        expect(screen.getByText('MacBook Air')).toBeInTheDocument();
    });
});