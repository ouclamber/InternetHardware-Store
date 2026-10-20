import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';

const mockNavigate = jest.fn();
const mockHttp = jest.fn();

jest.mock('shared/api/httpClient', () => ({
    http: mockHttp
}));

jest.mock('shared/lib/hoc/withNavigate', () => ({
    withNavigate: (Component) => (props) => (
        <Component {...props} navigate={mockNavigate} />
    )
}));

jest.mock('widgets/header/ui/Header', () => () => <div data-testid="header" />);
jest.mock('shared/ui/LoadingSpinner/LoadingSpinner', () => () => <div data-testid="loading" />);
jest.mock('shared/ui/ErrorMessage/ErrorMessage', () => ({ message }) => (
    <div data-testid="error">{message}</div>
));
jest.mock('entities/category/ui/CategoryCard', () => ({ category, onClick }) => (
    <div
        data-testid={`category-${category.id}`}
        onClick={() => onClick(category.id, category.name)}
    >
        {category.name}
    </div>
));

const mockCategories = [
    { Id: 1, Name: 'Ноутбуки', Description: 'Лучшие ноутбуки' },
    { Id: 2, Name: 'Компьютеры', Description: 'Мощные ПК' },
    { Id: 3, Name: 'Телевизоры', Description: '4K OLED' }
];

describe('HomePage', () => {
    let HomePage;

    beforeEach(() => {
        jest.clearAllMocks();
        localStorage.clear();
        jest.resetModules();

        // Заново импортируем после сброса модулей — сбрасываем CATEGORY_CACHE
        HomePage = require('./Home').default;
    });

    test('рендерит заголовок "Каталог"', () => {
        mockHttp.mockResolvedValue({ ok: true, json: async () => [] });
        render(<HomePage navigate={mockNavigate} />);
        expect(screen.getByText('Каталог')).toBeInTheDocument();
    });

    test('показывает подзаголовок', () => {
        mockHttp.mockResolvedValue({ ok: true, json: async () => [] });
        render(<HomePage navigate={mockNavigate} />);
        expect(screen.getByText(/Выберите интересующую/i)).toBeInTheDocument();
    });

    test('загружает категории с API', async () => {
        mockHttp.mockResolvedValueOnce({ ok: true, json: async () => mockCategories });
        mockHttp.mockResolvedValueOnce({ ok: true, json: async () => [] });

        render(<HomePage navigate={mockNavigate} />);

        await waitFor(() => {
            expect(screen.getByTestId('category-1')).toBeInTheDocument();
        });
    });

    test('клик по категории "Ноутбуки" → /Labtop', async () => {
        mockHttp.mockResolvedValueOnce({ ok: true, json: async () => mockCategories });
        mockHttp.mockResolvedValueOnce({ ok: true, json: async () => [] });

        render(<HomePage navigate={mockNavigate} />);

        await waitFor(() => {
            expect(screen.getByTestId('category-1')).toBeInTheDocument();
        });

        screen.getByTestId('category-1').click();
        expect(mockNavigate).toHaveBeenCalledWith('/Labtop');
    });

    test('клик по "Компьютеры" → /Computer', async () => {
        mockHttp.mockResolvedValueOnce({ ok: true, json: async () => mockCategories });
        mockHttp.mockResolvedValueOnce({ ok: true, json: async () => [] });

        render(<HomePage navigate={mockNavigate} />);

        await waitFor(() => {
            expect(screen.getByTestId('category-2')).toBeInTheDocument();
        });

        screen.getByTestId('category-2').click();
        expect(mockNavigate).toHaveBeenCalledWith('/Computer');
    });

    test('клик по "Телевизоры" → /Television', async () => {
        mockHttp.mockResolvedValueOnce({ ok: true, json: async () => mockCategories });
        mockHttp.mockResolvedValueOnce({ ok: true, json: async () => [] });

        render(<HomePage navigate={mockNavigate} />);

        await waitFor(() => {
            expect(screen.getByTestId('category-3')).toBeInTheDocument();
        });

        screen.getByTestId('category-3').click();
        expect(mockNavigate).toHaveBeenCalledWith('/Television');
    });

    test('показывает "Категории не найдены" при пустом ответе', async () => {
        mockHttp.mockResolvedValueOnce({ ok: true, json: async () => [] });
        mockHttp.mockResolvedValueOnce({ ok: true, json: async () => [] });

        render(<HomePage navigate={mockNavigate} />);

        await waitFor(() => {
            expect(screen.getByText('Категории не найдены')).toBeInTheDocument();
        });
    });

    test('показывает ошибку при сбое API', async () => {
        const consoleError = jest.spyOn(console, 'error').mockImplementation(() => {});
        mockHttp.mockRejectedValue(new Error('Server error'));

        render(<HomePage navigate={mockNavigate} />);

        await waitFor(() => {
            expect(screen.getByTestId('error')).toBeInTheDocument();
        });

        consoleError.mockRestore();
    });
});