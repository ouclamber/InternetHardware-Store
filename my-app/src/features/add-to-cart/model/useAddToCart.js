import { useContext } from 'react';
import { CartContext } from 'entities/cart/model/CartContext';
import { useNotification } from 'shared/lib/hooks/useNotification';

/**
 * Добавление товара в корзину с всплывающим уведомлением.
 * При ошибке показывается сообщение сервера (например, «Недостаточно товара на складе: доступно 2 шт.»)
 * вместо блокирующего alert().
 */
export const useAddToCart = () => {
    const context = useContext(CartContext);
    const { notification, showNotification } = useNotification();

    const addToCart = async (productId, quantity = 1, event, productName) => {
        if (event) event.stopPropagation();
        const result = await context.addToCart(productId, quantity);
        if (result.success) {
            showNotification('Товар добавлен в корзину!', 'success', productName || 'Товар');
            return { success: true };
        }

        showNotification(result.message || 'Не удалось добавить товар в корзину', 'error', productName || '');
        return { success: false, message: result.message };
    };

    return { addToCart, notification };
};
