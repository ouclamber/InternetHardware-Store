import React, { Component } from 'react';
import './Checkout.css';
import { withNavigate } from 'shared/lib/hoc/withNavigate';
import { CartContext } from 'entities/cart/model/CartContext';
import Header from 'widgets/header/ui/Header';
import MessageNotification from 'widgets/notification/ui/MessageNotification';
import LoadingSpinner from 'shared/ui/LoadingSpinner/LoadingSpinner';
import CheckoutSteps from './CheckoutSteps';
import Step1ContactInfo from './Step1ContactInfo';
import Step2Delivery from './Step2Delivery';
import Step3Payment from './Step3Payment';
import Step4Card from './Step4Card';
import Step5Confirmation from './Step5Confirmation';
import CheckoutSidebar from './CheckoutSidebar';
import CheckoutSuccess from './CheckoutSuccess';
import { http } from 'shared/api/httpClient';
import Configuration from 'shared/config/Configuration';

const pick = (obj, ...keys) => {
    for (const key of keys) {
        if (obj[key] !== undefined && obj[key] !== null) return obj[key];
    }
    return undefined;
};

class CheckoutPage extends Component {
    static contextType = CartContext;

    constructor(props) {
        super(props);
        this.state = {
            cartItems: [],
            loading: true,
            error: null,
            step: 1,
            formData: {
                firstName: '', lastName: '', email: '', phone: '',
                address: '', city: '', postalCode: '',
                deliveryMethod: 'courier', paymentMethod: 'card',
                comment: '', cardNumber: '', cardExpiry: '', cardCvv: ''
            },
            isSubmitting: false,
            orderSuccess: false,
            orderNumber: null,
            fieldErrors: { email: '', phone: '', cardNumber: '', cardExpiry: '', cardCvv: '' },
            notification: { show: false, message: '', type: 'success' }
        };
        this.abortController = null;
        this.notificationTimeout = null;
    }

    formatItem = (item) => {
        const product = item.product || item.Product;
        return {
            id: item.id ?? item.Id,
            productId: item.productId ?? item.ProductId,
            quantity: item.quantity ?? item.Quantity ?? 1,
            product: product ? {
                id: product.id ?? product.Id,
                name: product.name ?? product.Name,
                price: product.price ?? product.Price,
                images: product.images ?? product.Images ?? []
            } : null
        };
    };

    componentDidMount() {
        const passedCartItems = this.props.location?.state?.cartItems;
        if (passedCartItems?.length > 0) {
            this.setState({ cartItems: passedCartItems.map(this.formatItem), loading: false });
        } else if (this.context?.cartItems?.length > 0) {
            this.setState({ cartItems: this.context.cartItems.map(this.formatItem), loading: false });
        } else {
            this.loadCart();
        }
        this.loadUserData();
    }

    componentWillUnmount() {
        if (this.abortController) this.abortController.abort();
        if (this.notificationTimeout) clearTimeout(this.notificationTimeout);
    }

    showNotification = (message, type = 'success') => {
        if (this.notificationTimeout) clearTimeout(this.notificationTimeout);
        this.setState({ notification: { show: true, message, type } });
        this.notificationTimeout = setTimeout(() => {
            this.setState({ notification: { show: false, message: '', type: 'success' } });
        }, 3000);
    };

    validateEmail = (email) => /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/.test(email);

    validatePhone = (phone) => {
        const phoneRegex = /^(\+7|7|8)?[\s-]?\(?[0-9]{3}\)?[\s-]?[0-9]{3}[\s-]?[0-9]{2}[\s-]?[0-9]{2}$/;
        const cleanPhone = phone.replace(/[\s()]/g, '');
        const phoneRegexSimple = /^(\+7|7|8)?[0-9]{10}$/;
        return phoneRegex.test(phone) || phoneRegexSimple.test(cleanPhone);
    };

    formatPhone = (phone) => {
        const clean = phone.replace(/\D/g, '');
        if (clean.length === 11) {
            return `+${clean[0]} (${clean.slice(1, 4)}) ${clean.slice(4, 7)}-${clean.slice(7, 9)}-${clean.slice(9, 11)}`;
        }
        if (clean.length === 10) {
            return `+7 (${clean.slice(0, 3)}) ${clean.slice(3, 6)}-${clean.slice(6, 8)}-${clean.slice(8, 10)}`;
        }
        return phone;
    };

    validateName = (name) => name.trim().length >= 2 && /^[a-zA-Zа-яА-Я\s-]+$/.test(name);

    validateField = (name, value) => {
        const errors = { ...this.state.fieldErrors };
        switch (name) {
            case 'email':
                errors.email = !value ? 'Email обязателен'
                    : !this.validateEmail(value) ? 'Введите корректный email' : '';
                break;
            case 'phone':
                errors.phone = !value ? 'Телефон обязателен'
                    : !this.validatePhone(value) ? 'Введите корректный номер' : '';
                break;
            case 'firstName':
            case 'lastName':
                errors[name] = !value ? 'Поле обязательно'
                    : !this.validateName(value) ? 'Только буквы (минимум 2)' : '';
                break;
            default: break;
        }
        this.setState({ fieldErrors: errors });
        return !errors[name];
    };

    handleInputChange = (e) => {
        const { name, value } = e.target;
        this.setState(prev => ({
            formData: { ...prev.formData, [name]: value }
        }), () => this.validateField(name, value));
    };

    handlePhoneBlur = () => {
        const { phone } = this.state.formData;
        if (phone && this.validatePhone(phone)) {
            this.setState(prev => ({
                formData: { ...prev.formData, phone: this.formatPhone(phone) }
            }));
        }
    };

    handleCardNumberChange = (e) => {
        let value = e.target.value.replace(/\D/g, '');
        if (value.length > 16) value = value.slice(0, 16);
        let formattedValue = '';
        for (let i = 0; i < value.length; i++) {
            if (i > 0 && i % 4 === 0) formattedValue += ' ';
            formattedValue += value[i];
        }
        this.setState(prev => ({
            formData: { ...prev.formData, cardNumber: formattedValue },
            fieldErrors: {
                ...prev.fieldErrors,
                cardNumber: value.length > 0 && value.length !== 16 ? 'Номер карты должен содержать 16 цифр' : ''
            }
        }));
    };

    handleCardExpiryChange = (e) => {
        let value = e.target.value.replace(/\D/g, '');
        if (value.length > 4) value = value.slice(0, 4);
        let formattedValue = '';
        for (let i = 0; i < value.length; i++) {
            if (i === 2) formattedValue += '/';
            formattedValue += value[i];
        }
        let error = '';
        if (value.length === 4) {
            const month = parseInt(value.slice(0, 2));
            const year = parseInt(value.slice(2, 4));
            const currentYear = new Date().getFullYear() % 100;
            const currentMonth = new Date().getMonth() + 1;
            if (month < 1 || month > 12) error = 'Месяц от 01 до 12';
            else if (year < currentYear || (year === currentYear && month < currentMonth)) error = 'Срок истек';
        } else if (value.length > 0) error = 'Введите ММ/ГГ';
        this.setState(prev => ({
            formData: { ...prev.formData, cardExpiry: formattedValue },
            fieldErrors: { ...prev.fieldErrors, cardExpiry: error }
        }));
    };

    handleCardCvvChange = (e) => {
        let value = e.target.value.replace(/\D/g, '');
        if (value.length > 3) value = value.slice(0, 3);
        this.setState(prev => ({
            formData: { ...prev.formData, cardCvv: value },
            fieldErrors: {
                ...prev.fieldErrors,
                cardCvv: value.length > 0 && value.length !== 3 ? 'CVV должен содержать 3 цифры' : ''
            }
        }));
    };

    validateCardData = () => {
        const { cardNumber, cardExpiry, cardCvv } = this.state.formData;
        const cleanCardNumber = cardNumber.replace(/\s/g, '');
        let isValid = true;
        if (cleanCardNumber.length !== 16) {
            this.setState(prev => ({ fieldErrors: { ...prev.fieldErrors, cardNumber: 'Номер карты должен содержать 16 цифр' } }));
            isValid = false;
        }
        if (cardExpiry.length !== 5) {
            this.setState(prev => ({ fieldErrors: { ...prev.fieldErrors, cardExpiry: 'Введите ММ/ГГ' } }));
            isValid = false;
        }
        if (cardCvv.length !== 3) {
            this.setState(prev => ({ fieldErrors: { ...prev.fieldErrors, cardCvv: 'CVV должен содержать 3 цифры' } }));
            isValid = false;
        }
        return isValid;
    };

    loadCart = async () => {
        if (this.abortController) this.abortController.abort();
        this.abortController = new AbortController();
        try {
            this.setState({ loading: true });

            const response = await http(Configuration.Cart.Get, {
                signal: this.abortController.signal
            });
            const data = await response.json();

            const apiItems = pick(data, 'Items', 'items') || [];
            const cartItems = apiItems.map(item => {
                const productId = pick(item, 'ProductId', 'productId');
                const quantity = pick(item, 'Quantity', 'quantity') || 1;
                const productName = pick(item, 'ProductName', 'productName') || 'Товар';
                const unitPrice = pick(item, 'UnitPrice', 'unitPrice') || 0;
                const mainImage = pick(item, 'MainImage', 'mainImage');

                return {
                    id: pick(item, 'Id', 'id'),
                    productId,
                    quantity,
                    product: {
                        id: productId,
                        name: productName,
                        price: unitPrice,
                        images: mainImage && mainImage !== 'string'
                            ? [{ imageUrl: mainImage, isMain: true }]
                            : []
                    }
                };
            });

            this.setState({ cartItems, loading: false });
        } catch (error) {
            if (error.name === 'AbortError') return;
            this.setState({ error: error.message, loading: false });
        }
    };

    loadUserData = () => {
        const userName = localStorage.getItem('userName') || '';
        const parts = userName.split(' ');

        this.setState(prev => ({
            formData: {
                ...prev.formData,
                firstName: parts[0] || '',
                lastName: parts[1] || '',
                email: '',
                phone: ''
            }
        }));
    };

    handleNextStep = () => {
        const { step, formData } = this.state;
        if (step === 1) {
            if (!formData.firstName || !formData.lastName || !formData.email || !formData.phone) {
                this.showNotification('Заполните все обязательные поля', 'error'); return;
            }
            if (!this.validateName(formData.firstName)) { this.showNotification('Имя некорректно', 'error'); return; }
            if (!this.validateName(formData.lastName)) { this.showNotification('Фамилия некорректна', 'error'); return; }
            if (!this.validateEmail(formData.email)) { this.showNotification('Email некорректен', 'error'); return; }
            if (!this.validatePhone(formData.phone)) { this.showNotification('Телефон некорректен', 'error'); return; }
        }
        if (step === 2 && (!formData.address || !formData.city)) {
            this.showNotification('Заполните адрес доставки', 'error'); return;
        }
        if (step === 3 && formData.paymentMethod === 'card') {
            this.setState({ step: 4 }); window.scrollTo(0, 0); return;
        }
        if (step === 3 && formData.paymentMethod === 'cash') {
            this.setState({ step: 5 }); window.scrollTo(0, 0); return;
        }
        if (step === 4) {
            if (!this.validateCardData()) return;
            this.setState({ step: 5 }); window.scrollTo(0, 0); return;
        }
        this.setState({ step: step + 1 }); window.scrollTo(0, 0);
    };

    handlePrevStep = () => {
        const { step, formData } = this.state;
        if (step === 4) this.setState({ step: 3 });
        else if (step === 5) {
            if (formData.paymentMethod === 'card') this.setState({ step: 4 });
            else this.setState({ step: 3 });
        } else this.setState({ step: step - 1 });
        window.scrollTo(0, 0);
    };

    handleSubmitOrder = async () => {
        this.setState({ isSubmitting: true });
        try {
            const userId = localStorage.getItem('userId');
            const { cartItems, formData } = this.state;

            if (!userId) {
                this.showNotification('Пользователь не авторизован', 'error');
                this.setState({ isSubmitting: false });
                return;
            }
            if (cartItems.length === 0) {
                this.showNotification('Корзина пуста', 'error');
                this.setState({ isSubmitting: false });
                return;
            }

            const orderData = {
                firstName: formData.firstName,
                lastName: formData.lastName,
                email: formData.email,
                phone: formData.phone,
                address: formData.address,
                city: formData.city,
                postalCode: formData.postalCode || '',
                deliveryMethod: formData.deliveryMethod,
                paymentMethod: formData.paymentMethod,
                comment: formData.comment || ''
            };

            const response = await http.post(Configuration.Orders.Create, orderData);
            const result = await response.json();

            const orderNumber = result.orderId || result.id || '—';

            localStorage.setItem('needRefreshProfile', 'true');

            try {
                await http.delete(Configuration.Cart.Clear);
            } catch (e) {
                console.error('Ошибка очистки корзины:', e);
            }

            if (this.context?.updateCartCount) {
                await this.context.updateCartCount();
            }

            this.setState({ orderSuccess: true, orderNumber, step: 6, isSubmitting: false });
            this.showNotification(`Заказ оформлен! Номер: ${orderNumber}`, 'success');
        } catch (error) {
            console.error('Ошибка:', error);
            this.showNotification('Ошибка при оформлении заказа', 'error');
            this.setState({ isSubmitting: false });
        }
    };

    getTotalPrice = () => this.state.cartItems.reduce(
        (sum, item) => sum + (item.product?.price || 0) * item.quantity, 0
    );

    getDeliveryPrice = () => {
        const { formData } = this.state;
        const total = this.getTotalPrice();
        if (formData.deliveryMethod === 'courier') return total >= 3000 ? 0 : 300;
        if (formData.deliveryMethod === 'pickup') return 0;
        return 200;
    };

    getTotalWithDelivery = () => this.getTotalPrice() + this.getDeliveryPrice();

    renderCurrentStep = () => {
        const { step, formData, fieldErrors, cartItems } = this.state;

        switch (step) {
            case 1:
                return (
                    <Step1ContactInfo
                        formData={formData}
                        fieldErrors={fieldErrors}
                        onInputChange={this.handleInputChange}
                        onPhoneBlur={this.handlePhoneBlur}
                    />
                );
            case 2:
                return (
                    <Step2Delivery
                        formData={formData}
                        totalPrice={this.getTotalPrice()}
                        onInputChange={this.handleInputChange}
                    />
                );
            case 3:
                return (
                    <Step3Payment
                        formData={formData}
                        onInputChange={this.handleInputChange}
                    />
                );
            case 4:
                return (
                    <Step4Card
                        formData={formData}
                        fieldErrors={fieldErrors}
                        onCardNumberChange={this.handleCardNumberChange}
                        onCardExpiryChange={this.handleCardExpiryChange}
                        onCardCvvChange={this.handleCardCvvChange}
                    />
                );
            case 5:
                return (
                    <Step5Confirmation
                        cartItems={cartItems}
                        getTotalPrice={this.getTotalPrice}
                        getDeliveryPrice={this.getDeliveryPrice}
                        getTotalWithDelivery={this.getTotalWithDelivery}
                    />
                );
            default:
                return null;
        }
    };

    render() {
        const {
            loading, error, step, isSubmitting, cartItems, formData, notification, orderNumber
        } = this.state;

        if (loading) {
            return (
                <div className="checkout-page">
                    <LoadingSpinner text="Загрузка..." />
                </div>
            );
        }

        if (error || cartItems.length === 0) {
            return (
                <div className="checkout-page">
                    <div className="empty-cart">
                        <h2>Корзина пуста</h2>
                        <p>Добавьте товары в корзину, чтобы оформить заказ</p>
                        <button className="continue-shopping-btn" onClick={() => this.props.navigate('/HomePage')}>
                            Перейти в каталог
                        </button>
                    </div>
                </div>
            );
        }

        const isConfirmationStep = (step === 5) || (step === 4 && formData.paymentMethod === 'cash');
        const isSuccessStep = step === 6;
        const showBackButton = step > 1 && !isSuccessStep;
        const isCenteredStep = isConfirmationStep || isSuccessStep;

        return (
            <div className="checkout-page">
                <MessageNotification
                    show={notification.show}
                    message={notification.message}
                    type={notification.type}
                />
                <Header navigate={this.props.navigate} />

                <main className="main-content">
                    <div className="container checkout-container">
                        <div className="checkout-header">
                            <h1 className="checkout-title">Оформление заказа</h1>
                            <button className="back-button" onClick={() => this.props.navigate(-1)}>
                                Назад
                            </button>

                            <CheckoutSteps
                                step={step}
                                paymentMethod={formData.paymentMethod}
                                isConfirmationStep={isConfirmationStep}
                                isSuccessStep={isSuccessStep}
                            />
                        </div>

                        {isSuccessStep ? (
                            <CheckoutSuccess
                                orderNumber={orderNumber}
                                onContinueShopping={() => this.props.navigate('/HomePage')}
                            />
                        ) : isCenteredStep ? (
                            <div className="checkout-centered">
                                <div className="checkout-centered-step">
                                    <Step5Confirmation
                                        cartItems={cartItems}
                                        getTotalPrice={this.getTotalPrice}
                                        getDeliveryPrice={this.getDeliveryPrice}
                                        getTotalWithDelivery={this.getTotalWithDelivery}
                                    />
                                </div>
                            </div>
                        ) : (
                            <div className="checkout-content">
                                <div className="checkout-form">
                                    {this.renderCurrentStep()}
                                </div>
                                <div className="checkout-sidebar">
                                    <CheckoutSidebar
                                        cartItems={cartItems}
                                        getTotalPrice={this.getTotalPrice}
                                        getDeliveryPrice={this.getDeliveryPrice}
                                        getTotalWithDelivery={this.getTotalWithDelivery}
                                    />
                                </div>
                            </div>
                        )}

                        {!isSuccessStep && (
                            <div className={`checkout-actions ${isConfirmationStep ? 'checkout-actions--centered' : ''}`}>
                                {showBackButton && step < 5 && (
                                    <button className="btn-prev" onClick={this.handlePrevStep}>
                                        Назад
                                    </button>
                                )}
                                {step < 5 && (
                                    <button className="btn-next" onClick={this.handleNextStep}>
                                        Далее
                                    </button>
                                )}
                                {isConfirmationStep && (
                                    <button
                                        className="btn-submit"
                                        onClick={this.handleSubmitOrder}
                                        disabled={isSubmitting}
                                    >
                                        {isSubmitting ? 'Оформление...' : 'Подтвердить заказ'}
                                    </button>
                                )}
                            </div>
                        )}
                    </div>
                </main>
            </div>
        );
    }
}

export default withNavigate(CheckoutPage);