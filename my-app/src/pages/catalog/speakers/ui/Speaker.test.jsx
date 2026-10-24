import React from 'react';
import { render, screen } from '@testing-library/react';
import Speaker, { SPEAKER_TYPES, SPEAKER_BRANDS } from './Speaker';
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

describe('Speaker — страница категории «Колонки»', () => {
    beforeEach(() => jest.clearAllMocks());

    test('рендерит общий каталог с заголовком', () => {
        render(<Speaker />);
        expect(screen.getByText('Колонки')).toBeInTheDocument();
    });

    test('передаёт конфигурацию категории', () => {
        render(<Speaker />);

        expect(mockCatalog).toHaveBeenCalledWith(expect.objectContaining({
            navigate: mockNavigate,
            title: 'Колонки',
            categoryName: 'Колонки',
            placeholderText: 'Колонка',
            pageClassName: 'speakers-page',
            brandHints: SPEAKER_BRANDS,
            typeRules: SPEAKER_TYPES
        }));
    });

    test('правила типов распознают товар категории', () => {
        expect(detectProductType('jbl flip 6', SPEAKER_TYPES)).toBe('jbl-flip');
        expect(detectProductType('Неизвестный товар', SPEAKER_TYPES)).toBe('standard');
    });

    test('подсказки брендов не пустые', () => {
        expect(Object.keys(SPEAKER_BRANDS).length).toBeGreaterThan(0);
    });
});
