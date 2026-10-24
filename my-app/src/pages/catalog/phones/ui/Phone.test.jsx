import React from 'react';
import { render, screen } from '@testing-library/react';
import Phone, { PHONE_TYPES, PHONE_BRANDS } from './Phone';
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

describe('Phone — страница категории «Смартфоны»', () => {
    beforeEach(() => jest.clearAllMocks());

    test('рендерит общий каталог с заголовком', () => {
        render(<Phone />);
        expect(screen.getByText('Смартфоны')).toBeInTheDocument();
    });

    test('передаёт конфигурацию категории', () => {
        render(<Phone />);

        expect(mockCatalog).toHaveBeenCalledWith(expect.objectContaining({
            navigate: mockNavigate,
            title: 'Смартфоны',
            categoryName: 'Смартфоны',
            placeholderText: 'Смартфон',
            pageClassName: 'smartphones-page',
            brandHints: PHONE_BRANDS,
            typeRules: PHONE_TYPES
        }));
    });

    test('правила типов распознают товар категории', () => {
        expect(detectProductType('apple iphone 15 pro', PHONE_TYPES)).toBe('iphone');
        expect(detectProductType('Неизвестный товар', PHONE_TYPES)).toBe('standard');
    });

    test('подсказки брендов не пустые', () => {
        expect(Object.keys(PHONE_BRANDS).length).toBeGreaterThan(0);
    });
});
