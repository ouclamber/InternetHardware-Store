import React from 'react';
import { render, screen } from '@testing-library/react';
import Computer, { COMPUTER_TYPES, COMPUTER_BRANDS } from './Computer';
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

describe('Computer — страница категории «Компьютеры»', () => {
    beforeEach(() => jest.clearAllMocks());

    test('рендерит общий каталог с заголовком', () => {
        render(<Computer />);
        expect(screen.getByText('Компьютеры')).toBeInTheDocument();
    });

    test('передаёт конфигурацию категории', () => {
        render(<Computer />);

        expect(mockCatalog).toHaveBeenCalledWith(expect.objectContaining({
            navigate: mockNavigate,
            title: 'Компьютеры',
            categoryName: 'Компьютеры',
            placeholderText: 'Компьютер',
            pageClassName: 'computer-page',
            brandHints: COMPUTER_BRANDS,
            typeRules: COMPUTER_TYPES
        }));
    });

    test('правила типов распознают товар категории', () => {
        expect(detectProductType('игровой пк msi', COMPUTER_TYPES)).toBe('gaming');
        expect(detectProductType('Неизвестный товар', COMPUTER_TYPES)).toBe('standard');
    });

    test('подсказки брендов не пустые', () => {
        expect(Object.keys(COMPUTER_BRANDS).length).toBeGreaterThan(0);
    });
});
