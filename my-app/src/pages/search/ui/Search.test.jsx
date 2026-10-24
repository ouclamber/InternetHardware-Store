import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import Search from './Search';

const mockNavigate = jest.fn();
const mockAddToCart = jest.fn();
const mockHttp = jest.fn();

jest.mock('shared/api/httpClient', () => ({
    http: (...args) => mockHttp(...args)
}));

jest.mock('shared/lib/hoc/withNavigate', () => ({
    withNavigate: (Component) => (props) => (
        <Component {...props} navigate={mockNavigate} />
    )
}));

jest.mock('entities/cart/model/CartContext', () => {
    const ActualReact = jest.requireActual('react');
    return {
        CartContext: ActualReact.createContext({
            addToCart: (...args) => mockAddToCart(...args),
            cartItems: []
        })
    };
});

jest.mock('widgets/header/ui/Header', () => ({ onSearch }) => (
    <div data-testid="header">
        <button onClick={() => onSearch && onSearch('msi')}>Search</button>
    </div>
));
jest.mock('widgets/notification/ui/Notification', () => () => null);
jest.mock('shared/ui/LoadingSpinner/LoadingSpinner', () => () => (
    <div data-testid="loading" />
));
jest.mock('shared/ui/BackButton/BackButton', () => ({ onClick }) => (
    <button onClick={onClick}>Back</button>
));
jest.mock('entities/product/ui/ProductCard', () => ({ product, onClick }) => (
    <div
        data-testid={`product-${product.Id}`}
        onClick={() => onClick && onClick(product.Id)}
    >
        {product.Name}
    </div>
));

const mockProducts = [
    { Id: 1, Name: 'MSI Gaming', Price: 189999, CategoryId: 1 },
    { Id: 2, Name: 'ASUS ROG', Price: 129999, CategoryId: 1 },
    { Id: 3, Name: 'Samsung TV', Price: 109999, CategoryId: 2 }
];

describe('Search', () => {
    beforeEach(() => {
        jest.clearAllMocks();
        localStorage.clear();
        window.history.pushState({}, '', '/search');
        mockHttp.mockResolvedValue({ ok: true, json: async () => [] });
    });

    test('рендерит заголовок "Поиск товаров" без query', () => {
        render(<Search />);
        expect(screen.getByText('Поиск товаров')).toBeInTheDocument();
    });

    test('рендерит сайдбар с фильтрами', () => {
        render(<Search />);
        expect(screen.getByText('Цена')).toBeInTheDocument();
        expect(screen.getByText('Сортировка')).toBeInTheDocument();
    });

    test('рендерит BackButton', () => {
        render(<Search />);
        expect(screen.getByText('Back')).toBeInTheDocument();
    });

    test('клик на "Back" → navigate("/HomePage")', () => {
        render(<Search />);
        fireEvent.click(screen.getByText('Back'));
        expect(mockNavigate).toHaveBeenCalledWith('/HomePage');
    });

    test('пустой query не выполняет поиск', () => {
        render(<Search />);
        const searchCalls = mockHttp.mock.calls.filter(c =>
            c[0].includes('/search')
        );
        expect(searchCalls.length).toBe(0);
    });

    test('поиск с query — рендерит результаты', async () => {
        mockHttp.mockImplementation((url) => {
            if (url.includes('/search')) {
                return Promise.resolve({ ok: true, json: async () => mockProducts });
            }
            return Promise.resolve({ ok: true, json: async () => [] });
        });

        window.history.pushState({}, '', '/search?q=msi');
        render(<Search />);

        await waitFor(() => {
            expect(screen.getByTestId('product-1')).toBeInTheDocument();
        });
    });

    test('показывает "Найдено N товаров"', async () => {
        mockHttp.mockImplementation((url) => {
            if (url.includes('/search')) {
                return Promise.resolve({ ok: true, json: async () => mockProducts });
            }
            return Promise.resolve({ ok: true, json: async () => [] });
        });

        window.history.pushState({}, '', '/search?q=msi');
        render(<Search />);

        await waitFor(() => {
            expect(screen.getByText(/Найдено 3 товаров/)).toBeInTheDocument();
        });
    });

    test('рендерит результат поиска в заголовке', async () => {
        mockHttp.mockImplementation((url) => {
            if (url.includes('/search')) {
                return Promise.resolve({ ok: true, json: async () => mockProducts });
            }
            return Promise.resolve({ ok: true, json: async () => [] });
        });

        window.history.pushState({}, '', '/search?q=msi');
        render(<Search />);

        await waitFor(() => {
            expect(screen.getByText(/Результаты поиска: "msi"/)).toBeInTheDocument();
        });
    });

    test('фильтр по minPrice скрывает дешёвые', async () => {
        mockHttp.mockImplementation((url) => {
            if (url.includes('/search')) {
                return Promise.resolve({ ok: true, json: async () => mockProducts });
            }
            return Promise.resolve({ ok: true, json: async () => [] });
        });

        window.history.pushState({}, '', '/search?q=test');
        render(<Search />);

        await waitFor(() => {
            expect(screen.getByTestId('product-1')).toBeInTheDocument();
        });

        const minInput = screen.getByPlaceholderText('От');
        fireEvent.change(minInput, { target: { value: '150000' } });

        expect(screen.queryByTestId('product-2')).not.toBeInTheDocument();
    });

    test('фильтр по maxPrice скрывает дорогие', async () => {
        mockHttp.mockImplementation((url) => {
            if (url.includes('/search')) {
                return Promise.resolve({ ok: true, json: async () => mockProducts });
            }
            return Promise.resolve({ ok: true, json: async () => [] });
        });

        window.history.pushState({}, '', '/search?q=test');
        render(<Search />);

        await waitFor(() => {
            expect(screen.getByTestId('product-1')).toBeInTheDocument();
        });

        const maxInput = screen.getByPlaceholderText('До');
        fireEvent.change(maxInput, { target: { value: '150000' } });

        expect(screen.queryByTestId('product-1')).not.toBeInTheDocument();
    });

    test('сортировка price_desc — дорогие первыми', async () => {
        mockHttp.mockImplementation((url) => {
            if (url.includes('/search')) {
                return Promise.resolve({ ok: true, json: async () => mockProducts });
            }
            return Promise.resolve({ ok: true, json: async () => [] });
        });

        window.history.pushState({}, '', '/search?q=test');
        render(<Search />);

        await waitFor(() => {
            expect(screen.getByTestId('product-1')).toBeInTheDocument();
        });

        const select = screen.getByRole('combobox');
        fireEvent.change(select, { target: { value: 'price_desc' } });

        const cards = screen.getAllByTestId(/^product-/);
        expect(cards[0]).toHaveTextContent('MSI Gaming');
    });

    test('сортировка price_asc — дешёвые первыми', async () => {
        mockHttp.mockImplementation((url) => {
            if (url.includes('/search')) {
                return Promise.resolve({ ok: true, json: async () => mockProducts });
            }
            return Promise.resolve({ ok: true, json: async () => [] });
        });

        window.history.pushState({}, '', '/search?q=test');
        render(<Search />);

        await waitFor(() => {
            expect(screen.getByTestId('product-1')).toBeInTheDocument();
        });

        const select = screen.getByRole('combobox');
        fireEvent.change(select, { target: { value: 'price_asc' } });

        const cards = screen.getAllByTestId(/^product-/);
        expect(cards[0]).toHaveTextContent('Samsung TV');
    });

    test('кнопка "Сбросить фильтры" появляется при активных фильтрах', async () => {
        render(<Search />);
        const minInput = screen.getByPlaceholderText('От');
        fireEvent.change(minInput, { target: { value: '10000' } });

        expect(screen.getByText('Сбросить все фильтры')).toBeInTheDocument();
    });

    test('клик "Сбросить фильтры" очищает', async () => {
        render(<Search />);
        const minInput = screen.getByPlaceholderText('От');
        fireEvent.change(minInput, { target: { value: '10000' } });

        fireEvent.click(screen.getByText('Сбросить все фильтры'));

        expect(minInput).toHaveValue(null);
    });

    test('клик на товар → navigate("/product/1")', async () => {
        mockHttp.mockImplementation((url) => {
            if (url.includes('/search')) {
                return Promise.resolve({ ok: true, json: async () => mockProducts });
            }
            return Promise.resolve({ ok: true, json: async () => [] });
        });

        window.history.pushState({}, '', '/search?q=test');
        render(<Search />);

        await waitFor(() => {
            expect(screen.getByTestId('product-1')).toBeInTheDocument();
        });

        fireEvent.click(screen.getByTestId('product-1'));
        expect(mockNavigate).toHaveBeenCalledWith('/product/1');
    });

    test('пустой результат → "Ничего не найдено"', async () => {
        mockHttp.mockImplementation((url) => {
            if (url.includes('/search')) {
                return Promise.resolve({ ok: true, json: async () => [] });
            }
            return Promise.resolve({ ok: true, json: async () => [] });
        });

        window.history.pushState({}, '', '/search?q=xyz');
        render(<Search />);

        await waitFor(() => {
            expect(screen.getByText('Ничего не найдено')).toBeInTheDocument();
        });
    });

    test('ошибка поиска → error-message', async () => {
        const consoleError = jest.spyOn(console, 'error').mockImplementation(() => {});
        mockHttp.mockImplementation((url) => {
            if (url.includes('/search')) {
                return Promise.reject(new Error('Server error'));
            }
            return Promise.resolve({ ok: true, json: async () => [] });
        });

        window.history.pushState({}, '', '/search?q=test');
        render(<Search />);

        await waitFor(() => {
            expect(screen.getByText('Ошибка при выполнении поиска')).toBeInTheDocument();
        });

        consoleError.mockRestore();
    });

    test('loading во время поиска', async () => {
        mockHttp.mockImplementation((url) => {
            if (url.includes('/search')) {
                return new Promise(() => {});
            }
            return Promise.resolve({ ok: true, json: async () => [] });
        });

        window.history.pushState({}, '', '/search?q=test');
        render(<Search />);

        await waitFor(() => {
            expect(screen.getByTestId('loading')).toBeInTheDocument();
        });
    });

    test('рендерит пагинацию при 10 товарах', async () => {
        const manyProducts = Array(10).fill(null).map((_, i) => ({
            Id: i + 1,
            Name: `Товар ${i + 1}`,
            Price: 1000 + i * 100,
            CategoryId: 1
        }));

        mockHttp.mockImplementation((url) => {
            if (url.includes('/search')) {
                return Promise.resolve({ ok: true, json: async () => manyProducts });
            }
            return Promise.resolve({ ok: true, json: async () => [] });
        });

        window.history.pushState({}, '', '/search?q=test');
        render(<Search />);

        await waitFor(() => {
            expect(screen.getByText(/Найдено 10/)).toBeInTheDocument();
        });

        expect(screen.getByText('1')).toBeInTheDocument();
        expect(screen.getByText('2')).toBeInTheDocument();
    });

    test('клик на страницу 2 меняет содержимое', async () => {
        const manyProducts = Array(10).fill(null).map((_, i) => ({
            Id: i + 1,
            Name: `Товар ${i + 1}`,
            Price: 1000 + i * 100,
            CategoryId: 1
        }));

        mockHttp.mockImplementation((url) => {
            if (url.includes('/search')) {
                return Promise.resolve({ ok: true, json: async () => manyProducts });
            }
            return Promise.resolve({ ok: true, json: async () => [] });
        });

        window.history.pushState({}, '', '/search?q=test');
        render(<Search />);

        await waitFor(() => {
            expect(screen.getByTestId('product-1')).toBeInTheDocument();
        });

        const page2 = screen.getByText('2');
        fireEvent.click(page2);

        await waitFor(() => {
            expect(screen.getByTestId('product-7')).toBeInTheDocument();
        });
    });

    test('сортировка name_asc сортирует по имени', async () => {
        mockHttp.mockImplementation((url) => {
            if (url.includes('/search')) {
                return Promise.resolve({ ok: true, json: async () => mockProducts });
            }
            return Promise.resolve({ ok: true, json: async () => [] });
        });

        window.history.pushState({}, '', '/search?q=test');
        render(<Search />);

        await waitFor(() => {
            expect(screen.getByTestId('product-1')).toBeInTheDocument();
        });

        const select = screen.getByRole('combobox');
        fireEvent.change(select, { target: { value: 'name_asc' } });

        const cards = screen.getAllByTestId(/^product-/);
        expect(cards[0]).toHaveTextContent('ASUS ROG');
    });
});