import React from 'react';
import { render, screen } from '@testing-library/react';
import Laptop, { LAPTOP_TYPES, LAPTOP_BRANDS } from './Laptop';
import { detectProductType } from 'entities/product/lib/detectProductType';

const mockNavigate = jest.fn();
const mockCatalog = jest.fn();

jest.mock('shared/lib/hoc/withNavigate', () => ({
    withNavigate: (Component) => (props) => <Component {...props} navigate={mockNavigate} />
}));

jest.mock('widgets/category-catalog/ui/CategoryCatalog', () => (props) => {
    mockCatalog(props);
    return <h1>{props.title}</h1>;
});

describe('Laptop — страница категории «Ноутбуки»', () => {
    beforeEach(() => jest.clearAllMocks());

    test('рендерит общий каталог с заголовком', () => {
        render(<Laptop />);
        expect(screen.getByText('Ноутбуки')).toBeInTheDocument();
    });

    test('передаёт конфигурацию категории', () => {
        render(<Laptop />);

        expect(mockCatalog).toHaveBeenCalledWith(expect.objectContaining({
            navigate: mockNavigate,
            title: 'Ноутбуки',
            categoryName: 'Ноутбуки',
            placeholderText: 'Ноутбук',
            pageClassName: 'laptop-page',
            brandHints: LAPTOP_BRANDS,
            typeRules: LAPTOP_TYPES
        }));
    });

    test('правила типов распознают товар категории', () => {
        expect(detectProductType('asus tuf gaming f15', LAPTOP_TYPES)).toBe('gaming');
        expect(detectProductType('Неизвестный товар', LAPTOP_TYPES)).toBe('standard');
    });

    test('подсказки брендов не пустые', () => {
        expect(Object.keys(LAPTOP_BRANDS).length).toBeGreaterThan(0);
    });
});
