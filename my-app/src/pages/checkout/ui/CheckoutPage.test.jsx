import React from 'react';
import { render, screen, fireEvent, act } from '@testing-library/react';
import CheckoutPage from './Checkout'; // ← фактическое имя файла (Checkout.jsx)
import { CartContext } from 'entities/cart/model/CartContext';
import { http } from 'shared/api/httpClient';
import Configuration from 'shared/config/Configuration';

const mockNavigate = jest.fn();

jest.mock('shared/api/httpClient', () => ({
    http: Object.assign(
        jest.fn(),
        {
            get: jest.fn(),
            post: jest.fn(),
            put: jest.fn(),
            delete: jest.fn(),
        }
    ),
}));

jest.mock('shared/lib/hoc/withNavigate', () => ({
    withNavigate: (Component) => (props) => (
        <Component {...props} navigate={mockNavigate} />
    ),
}));

jest.mock('widgets/header/ui/Header', () => () => <div data-testid="header" />);
jest.mock('widgets/notification/ui/MessageNotification', () => ({ show, message, type }) =>
    show ? <div data-testid="notification" data-type={type}>{message}</div> : null
);
jest.mock('shared/ui/LoadingSpinner/LoadingSpinner', () => ({ text }) => (
    <div data-testid="loading-spinner">{text}</div>
));
jest.mock('./CheckoutSteps', () => () => <div data-testid="checkout-steps" />);
jest.mock('./Step1ContactInfo', () => ({ formData, fieldErrors, onInputChange, onPhoneBlur }) => (
    <div data-testid="step1">
        <input name="firstName" value={formData.firstName} onChange={onInputChange} placeholder="Имя" />
        <input name="lastName" value={formData.lastName} onChange={onInputChange} placeholder="Фамилия" />
        <input name="email" value={formData.email} onChange={onInputChange} placeholder="Email" />
        <input name="phone" value={formData.phone} onChange={onInputChange} onBlur={onPhoneBlur} placeholder="Телефон" />
        {fieldErrors.email && <span data-testid="err-email">{fieldErrors.email}</span>}
    </div>
));
jest.mock('./Step2Delivery', () => ({ formData, onInputChange }) => (
    <div data-testid="step2">
        <input name="address" value={formData.address} onChange={onInputChange} placeholder="Адрес" />
        <input name="city" value={formData.city} onChange={onInputChange} placeholder="Город" />
    </div>
));
jest.mock('./Step3Payment', () => ({ formData, onInputChange }) => (
    <div data-testid="step3">
        <label>Карта<input type="radio" name="paymentMethod" value="card" checked={formData.paymentMethod === 'card'} onChange={onInputChange} /></label>
        <label>Наличные<input type="radio" name="paymentMethod" value="cash" checked={formData.paymentMethod === 'cash'} onChange={onInputChange} /></label>
    </div>
));
jest.mock('./Step4Card', () => ({ formData, fieldErrors, onCardNumberChange, onCardExpiryChange, onCardCvvChange }) => (
    <div data-testid="step4">
        <input data-testid="cardNumber" value={formData.cardNumber} onChange={onCardNumberChange} />
        <input data-testid="cardExpiry" value={formData.cardExpiry} onChange={onCardExpiryChange} />
        <input data-testid="cardCvv" value={formData.cardCvv} onChange={onCardCvvChange} />
        {fieldErrors.cardNumber && <span data-testid="err-card">{fieldErrors.cardNumber}</span>}
    </div>
));
jest.mock('./Step5Confirmation', () => ({ cartItems, getTotalWithDelivery }) => (
    <div data-testid="step5">
        <span data-testid="total-with-delivery">{getTotalWithDelivery()}</span>
        <span data-testid="items-count">{cartItems.length}</span>
    </div>
));
jest.mock('./CheckoutSidebar', () => () => <aside data-testid="sidebar" />);
jest.mock('./CheckoutSuccess', () => ({ orderNumber, onContinueShopping }) => (
    <div data-testid="success">
        <span data-testid="order-number">{orderNumber}</span>
        <button onClick={onContinueShopping}>Продолжить покупки</button>
    </div>
));

const createMockContext = (overrides = {}) => ({
    cartItems: [],
    cartCount: 0,
    loading: false,
    addToCart: jest.fn(),
    updateCartItem: jest.fn(),
    removeFromCart: jest.fn(),
    clearCart: jest.fn().mockResolvedValue({ success: true }),
    updateCartCount: jest.fn().mockResolvedValue(),
    refreshCart: jest.fn().mockResolvedValue(),
    ...overrides,
});

const renderCheckout = (ctx = createMockContext(), props = {}) =>
    render(
        <CartContext.Provider value={ctx}>
            <CheckoutPage
                navigate={mockNavigate}
                location={{ state: null }}
                {...props}
            />
        </CartContext.Provider>
    );

const mockJsonResponse = (data, ok = true) => ({
    ok,
    status: ok ? 200 : 400,
    json: jest.fn().mockResolvedValue(data),
});

const sampleCartApi = {
    Items: [
        { Id: 1, ProductId: 10, Quantity: 2, ProductName: 'Товар A', UnitPrice: 1000, MainImage: '/img/a.jpg' },
        { Id: 2, ProductId: 20, Quantity: 1, ProductName: 'Товар B', UnitPrice: 500 },
    ],
};

const setupHttp = ({ cartResponse = sampleCartApi, orderResponse = { orderId: 'ORD-777' }, clearOk = true } = {}) => {
    http.mockImplementation((url) => {
        if (url === Configuration.Cart.Get) return Promise.resolve(mockJsonResponse(cartResponse));
        return Promise.reject(new Error(`Unexpected GET: ${url}`));
    });
    http.post.mockResolvedValue(mockJsonResponse(orderResponse));
    http.delete.mockResolvedValue(mockJsonResponse({ success: clearOk }));
};

const fillStep1Valid = () => {
    fireEvent.change(screen.getByPlaceholderText('Имя'), { target: { name: 'firstName', value: 'Иван' } });
    fireEvent.change(screen.getByPlaceholderText('Фамилия'), { target: { name: 'lastName', value: 'Петров' } });
    fireEvent.change(screen.getByPlaceholderText('Email'), { target: { name: 'email', value: 'test@example.com' } });
    fireEvent.change(screen.getByPlaceholderText('Телефон'), { target: { name: 'phone', value: '+79991234567' } });
};

const goToStep2 = () => {
    fillStep1Valid();
    fireEvent.click(screen.getByRole('button', { name: 'Далее' }));
};

const goToConfirmationCash = () => {
    goToStep2();
    fireEvent.change(screen.getByPlaceholderText('Адрес'), { target: { name: 'address', value: 'ул. Ленина 1' } });
    fireEvent.change(screen.getByPlaceholderText('Город'), { target: { name: 'city', value: 'Москва' } });
    fireEvent.click(screen.getByRole('button', { name: 'Далее' })); // step3
    fireEvent.click(screen.getByLabelText('Наличные'));
    fireEvent.click(screen.getByRole('button', { name: 'Далее' })); // step5
};

describe('CheckoutPage', () => {
    beforeEach(() => {
        jest.clearAllMocks();
        localStorage.clear();
        window.alert = jest.fn();
        window.confirm = jest.fn(() => true);
        window.scrollTo = jest.fn();
        jest.spyOn(console, 'error').mockImplementation(() => {});
        jest.spyOn(console, 'log').mockImplementation(() => {});
        setupHttp();
    });

    afterEach(() => {
        jest.restoreAllMocks();
    });

    test('загружает корзину через API, если контекст и location.state пусты', async () => {
        renderCheckout();
        expect(screen.getByTestId('loading-spinner')).toBeInTheDocument();
        expect(await screen.findByTestId('step1')).toBeInTheDocument();
        expect(http).toHaveBeenCalledWith(Configuration.Cart.Get, expect.any(Object));
    });

    test('использует cartItems из location.state', async () => {
        const stateItems = [{
            id: 1, productId: 10, quantity: 2,
            product: { id: 10, name: 'X', price: 500, images: [] },
        }];
        renderCheckout(createMockContext(), { location: { state: { cartItems: stateItems } } });
        await screen.findByTestId('step1');
        expect(http).not.toHaveBeenCalledWith(Configuration.Cart.Get, expect.anything());
    });

    test('использует cartItems из CartContext, если location.state пуст', async () => {
        const ctx = createMockContext({
            cartItems: [{ id: 1, productId: 10, quantity: 1, product: { id: 10, name: 'Y', price: 100, images: [] } }],
        });
        renderCheckout(ctx);
        await screen.findByTestId('step1');
        expect(http).not.toHaveBeenCalledWith(Configuration.Cart.Get, expect.anything());
    });

    test('пустая корзина: показывает "Корзина пуста"', async () => {
        setupHttp({ cartResponse: { Items: [] } });
        renderCheckout();
        expect(await screen.findByText('Корзина пуста')).toBeInTheDocument();
        expect(screen.getByText('Перейти в каталог')).toBeInTheDocument();
    });

    test('ошибка загрузки → тоже "Корзина пуста"', async () => {
        http.mockRejectedValueOnce(new Error('Network'));
        renderCheckout();
        expect(await screen.findByText('Корзина пуста')).toBeInTheDocument();
    });

    test('клик "Перейти в каталог" → navigate("/HomePage")', async () => {
        setupHttp({ cartResponse: { Items: [] } });
        renderCheckout();
        await screen.findByText('Корзина пуста');
        fireEvent.click(screen.getByText('Перейти в каталог'));
        expect(mockNavigate).toHaveBeenCalledWith('/HomePage');
    });

    test('ошибка email для некорректного значения', async () => {
        renderCheckout();
        await screen.findByTestId('step1');
        fireEvent.change(screen.getByPlaceholderText('Email'), { target: { name: 'email', value: 'bad' } });
        expect(screen.getByTestId('err-email')).toHaveTextContent('Введите корректный email');
    });

    test('"Далее" не переключает шаг, если поля пусты', async () => {
        renderCheckout();
        await screen.findByTestId('step1');
        await act(async () => {
            fireEvent.click(screen.getByRole('button', { name: 'Далее' }));
        });
        expect(screen.getByTestId('step1')).toBeInTheDocument();
        expect(screen.getByTestId('notification')).toHaveTextContent('Заполните все обязательные поля');
    });

    test('"Далее" не переключает шаг при некорректном email', async () => {
        renderCheckout();
        await screen.findByTestId('step1');
        fillStep1Valid();
        fireEvent.change(screen.getByPlaceholderText('Email'), { target: { name: 'email', value: 'bad' } });
        await act(async () => {
            fireEvent.click(screen.getByRole('button', { name: 'Далее' }));
        });
        expect(screen.getByTestId('step1')).toBeInTheDocument();
        expect(screen.getByTestId('notification')).toHaveTextContent('Email некорректен');
    });

    test('"Далее" не переключает шаг при некорректном телефоне', async () => {
        renderCheckout();
        await screen.findByTestId('step1');
        fillStep1Valid();
        fireEvent.change(screen.getByPlaceholderText('Телефон'), { target: { name: 'phone', value: '123' } });
        await act(async () => {
            fireEvent.click(screen.getByRole('button', { name: 'Далее' }));
        });
        expect(screen.getByTestId('step1')).toBeInTheDocument();
        expect(screen.getByTestId('notification')).toHaveTextContent('Телефон некорректен');
    });

    test('переход на шаг 2 при валидных данных', async () => {
        renderCheckout();
        await screen.findByTestId('step1');
        goToStep2();
        expect(screen.getByTestId('step2')).toBeInTheDocument();
    });

    test('шаг 2: требует адрес и город', async () => {
        renderCheckout();
        await screen.findByTestId('step1');
        goToStep2();
        await act(async () => {
            fireEvent.click(screen.getByRole('button', { name: 'Далее' }));
        });
        expect(screen.getByTestId('step2')).toBeInTheDocument();
        expect(screen.getByTestId('notification')).toHaveTextContent('Заполните адрес доставки');
    });

    test('шаг 2 → шаг 3 при заполненных полях', async () => {
        renderCheckout();
        await screen.findByTestId('step1');
        goToStep2();
        fireEvent.change(screen.getByPlaceholderText('Адрес'), { target: { name: 'address', value: 'A' } });
        fireEvent.change(screen.getByPlaceholderText('Город'), { target: { name: 'city', value: 'M' } });
        fireEvent.click(screen.getByRole('button', { name: 'Далее' }));
        expect(screen.getByTestId('step3')).toBeInTheDocument();
    });

    test('шаг 3: выбор "Наличные" → сразу шаг 5', async () => {
        renderCheckout();
        await screen.findByTestId('step1');
        goToStep2();
        fireEvent.change(screen.getByPlaceholderText('Адрес'), { target: { name: 'address', value: 'A' } });
        fireEvent.change(screen.getByPlaceholderText('Город'), { target: { name: 'city', value: 'M' } });
        fireEvent.click(screen.getByRole('button', { name: 'Далее' }));
        fireEvent.click(screen.getByLabelText('Наличные'));
        fireEvent.click(screen.getByRole('button', { name: 'Далее' }));
        expect(screen.getByTestId('step5')).toBeInTheDocument();
    });

    test('шаг 3: выбор "Карта" → шаг 4', async () => {
        renderCheckout();
        await screen.findByTestId('step1');
        goToStep2();
        fireEvent.change(screen.getByPlaceholderText('Адрес'), { target: { name: 'address', value: 'A' } });
        fireEvent.change(screen.getByPlaceholderText('Город'), { target: { name: 'city', value: 'M' } });
        fireEvent.click(screen.getByRole('button', { name: 'Далее' }));
        fireEvent.click(screen.getByLabelText('Карта'));
        fireEvent.click(screen.getByRole('button', { name: 'Далее' }));
        expect(screen.getByTestId('step4')).toBeInTheDocument();
    });

    test('форматирует номер карты (4-4-4-4)', async () => {
        renderCheckout();
        await screen.findByTestId('step1');
        goToStep2();
        fireEvent.change(screen.getByPlaceholderText('Адрес'), { target: { name: 'address', value: 'A' } });
        fireEvent.change(screen.getByPlaceholderText('Город'), { target: { name: 'city', value: 'M' } });
        fireEvent.click(screen.getByRole('button', { name: 'Далее' }));
        fireEvent.click(screen.getByLabelText('Карта'));
        fireEvent.click(screen.getByRole('button', { name: 'Далее' }));
        const cardInput = screen.getByTestId('cardNumber');
        fireEvent.change(cardInput, { target: { value: '4111111111111111' } });
        expect(cardInput.value).toBe('4111 1111 1111 1111');
    });

    test('форматирует срок ММ/ГГ', async () => {
        renderCheckout();
        await screen.findByTestId('step1');
        goToStep2();
        fireEvent.change(screen.getByPlaceholderText('Адрес'), { target: { name: 'address', value: 'A' } });
        fireEvent.change(screen.getByPlaceholderText('Город'), { target: { name: 'city', value: 'M' } });
        fireEvent.click(screen.getByRole('button', { name: 'Далее' }));
        fireEvent.click(screen.getByLabelText('Карта'));
        fireEvent.click(screen.getByRole('button', { name: 'Далее' }));
        const expInput = screen.getByTestId('cardExpiry');
        fireEvent.change(expInput, { target: { value: '1230' } });
        expect(expInput.value).toBe('12/30');
    });

    test('невалидная карта: показывает ошибку и остаётся на шаге 4', async () => {
        renderCheckout();
        await screen.findByTestId('step1');
        goToStep2();
        fireEvent.change(screen.getByPlaceholderText('Адрес'), { target: { name: 'address', value: 'A' } });
        fireEvent.change(screen.getByPlaceholderText('Город'), { target: { name: 'city', value: 'M' } });
        fireEvent.click(screen.getByRole('button', { name: 'Далее' }));
        fireEvent.click(screen.getByLabelText('Карта'));
        fireEvent.click(screen.getByRole('button', { name: 'Далее' }));
        await act(async () => {
            fireEvent.click(screen.getByRole('button', { name: 'Далее' }));
        });
        expect(screen.getByTestId('step4')).toBeInTheDocument();
        expect(screen.getByTestId('err-card')).toHaveTextContent('Номер карты должен содержать 16 цифр');
    });

    test('неавторизован: показывает "Пользователь не авторизован"', async () => {
        renderCheckout();
        await screen.findByTestId('step1');
        goToConfirmationCash();
        await act(async () => {
            fireEvent.click(screen.getByRole('button', { name: 'Подтвердить заказ' }));
        });
        expect(screen.getByTestId('notification')).toHaveTextContent('Пользователь не авторизован');
    });

    test('успех: http.post, http.delete, orderSuccess, orderNumber', async () => {
        localStorage.setItem('userId', '42');
        localStorage.setItem('userName', 'Иван Петров');
        const ctx = createMockContext();
        renderCheckout(ctx);
        await screen.findByTestId('step1');
        goToConfirmationCash();

        await act(async () => {
            fireEvent.click(screen.getByRole('button', { name: 'Подтвердить заказ' }));
        });

        expect(http.post).toHaveBeenCalledWith(
            Configuration.Orders.Create,
            expect.objectContaining({ firstName: 'Иван', lastName: 'Петров' })
        );
        // Корзину очищает сервер в той же транзакции, что и создание заказа
        expect(http.delete).not.toHaveBeenCalled();
        expect(screen.getByTestId('success')).toBeInTheDocument();
        expect(screen.getByTestId('order-number')).toHaveTextContent('ORD-777');
        expect(localStorage.getItem('needRefreshProfile')).toBe('true');
        expect(ctx.updateCartCount).toHaveBeenCalled();
    });

    test('ошибка http.post → notification "Ошибка при оформлении заказа"', async () => {
        localStorage.setItem('userId', '42');
        http.post.mockRejectedValueOnce(new Error('Server error'));
        renderCheckout();
        await screen.findByTestId('step1');
        goToConfirmationCash();

        await act(async () => {
            fireEvent.click(screen.getByRole('button', { name: 'Подтвердить заказ' }));
        });

        expect(screen.getByTestId('notification')).toHaveTextContent('Ошибка при оформлении заказа');
        expect(screen.getByTestId('notification')).toHaveAttribute('data-type', 'error');
    });

    test('"Продолжить покупки" → navigate("/HomePage")', async () => {
        localStorage.setItem('userId', '42');
        renderCheckout();
        await screen.findByTestId('step1');
        goToConfirmationCash();
        await act(async () => {
            fireEvent.click(screen.getByRole('button', { name: 'Подтвердить заказ' }));
        });
        await screen.findByTestId('success');
        fireEvent.click(screen.getByText('Продолжить покупки'));
        expect(mockNavigate).toHaveBeenCalledWith('/HomePage');
    });

    test('клик "Назад" → navigate(-1)', async () => {
        renderCheckout();
        await screen.findByTestId('step1');
        fireEvent.click(screen.getByRole('button', { name: 'Назад' }));
        expect(mockNavigate).toHaveBeenCalledWith(-1);
    });

    test('курьер: бесплатно при сумме ≥ 3000', async () => {
        const ctx = createMockContext({
            cartItems: [{ id: 1, productId: 1, quantity: 2, product: { id: 1, name: 'A', price: 2000, images: [] } }],
        });
        renderCheckout(ctx);
        await screen.findByTestId('step1');
        goToConfirmationCash();
        expect(screen.getByTestId('total-with-delivery')).toHaveTextContent('4000');
    });

    test('курьер: +300 при сумме < 3000', async () => {
        const ctx = createMockContext({
            cartItems: [{ id: 1, productId: 1, quantity: 1, product: { id: 1, name: 'A', price: 1000, images: [] } }],
        });
        renderCheckout(ctx);
        await screen.findByTestId('step1');
        goToConfirmationCash();
        expect(screen.getByTestId('total-with-delivery')).toHaveTextContent('1300');
    });

    test('ошибка сервера (нет на складе) — показывается текст сервера и корзина обновляется', async () => {
        localStorage.setItem('userId', '42');
        const serverError = new Error('Недостаточно товара на складе: доступно 1 шт.');
        serverError.status = 400;
        serverError.data = { error: 'Недостаточно товара на складе: доступно 1 шт.', type: 'DomainError' };
        http.post.mockRejectedValueOnce(serverError);

        const ctx = createMockContext();
        renderCheckout(ctx);
        await screen.findByTestId('step1');
        goToConfirmationCash();

        await act(async () => {
            fireEvent.click(screen.getByRole('button', { name: 'Подтвердить заказ' }));
        });

        expect(screen.getByTestId('notification')).toHaveTextContent('Недостаточно товара на складе: доступно 1 шт.');
        expect(ctx.refreshCart).toHaveBeenCalled();
    });
});
