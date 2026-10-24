import React from 'react';
import { render, screen } from '@testing-library/react';
import Television, { TELEVISION_TYPES, TELEVISION_BRANDS } from './Television';
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

describe('Television — страница категории «Телевизоры»', () => {
    beforeEach(() => jest.clearAllMocks());

    test('рендерит общий каталог с заголовком', () => {
        render(<Television />);
        expect(screen.getByText('Телевизоры')).toBeInTheDocument();
    });

    test('передаёт конфигурацию категории', () => {
        render(<Television />);

        expect(mockCatalog).toHaveBeenCalledWith(expect.objectContaining({
            navigate: mockNavigate,
            title: 'Телевизоры',
            categoryName: 'Телевизоры',
            placeholderText: 'Телевизор',
            pageClassName: 'television-page',
            brandHints: TELEVISION_BRANDS,
            typeRules: TELEVISION_TYPES
        }));
    });

    test('правила типов распознают товар категории', () => {
        expect(detectProductType('lg oled55c3', TELEVISION_TYPES)).toBe('lg');
        expect(detectProductType('Неизвестный товар', TELEVISION_TYPES)).toBe('standard');
    });

    test('подсказки брендов не пустые', () => {
        expect(Object.keys(TELEVISION_BRANDS).length).toBeGreaterThan(0);
    });
});
