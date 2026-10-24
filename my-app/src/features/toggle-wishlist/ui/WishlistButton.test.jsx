import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import WishlistButton from './WishlistButton';
import { useWishlist } from 'entities/wishlist/model/WishlistContext';

jest.mock('entities/wishlist/model/WishlistContext', () => ({
    useWishlist: jest.fn()
}));

const setup = ({ inWishlist = false, toggleResult = { success: true, added: true }, isAuthenticated = true } = {}) => {
    const toggle = jest.fn().mockResolvedValue(toggleResult);
    useWishlist.mockReturnValue({ isInWishlist: () => inWishlist, toggle, isAuthenticated });
    return toggle;
};

describe('WishlistButton', () => {
    beforeEach(() => jest.clearAllMocks());

    test('неактивное состояние', () => {
        setup();
        render(<WishlistButton productId={1} />);

        const btn = screen.getByRole('button', { name: 'Добавить в избранное' });
        expect(btn).toHaveAttribute('aria-pressed', 'false');
    });

    test('активное состояние', () => {
        setup({ inWishlist: true });
        render(<WishlistButton productId={1} />);

        expect(screen.getByRole('button', { name: 'Убрать из избранного' })).toHaveAttribute('aria-pressed', 'true');
    });

    test('клик переключает и не всплывает до карточки', async () => {
        const toggle = setup();
        const onCardClick = jest.fn();
        const onResult = jest.fn();

        render(
            <div onClick={onCardClick}>
                <WishlistButton productId={42} onResult={onResult} />
            </div>
        );
        fireEvent.click(screen.getByRole('button'));

        await waitFor(() => expect(onResult).toHaveBeenCalledWith({ success: true, added: true }));
        expect(toggle).toHaveBeenCalledWith(42);
        expect(onCardClick).not.toHaveBeenCalled();
    });

    test('большой вариант показывает текст', () => {
        setup({ inWishlist: true });
        render(<WishlistButton productId={1} size="large" />);

        expect(screen.getByText('В избранном')).toBeInTheDocument();
    });

    test('гостю подсказывает войти', () => {
        setup({ isAuthenticated: false });
        render(<WishlistButton productId={1} />);

        expect(screen.getByRole('button')).toHaveAttribute('title', 'Войдите, чтобы добавлять в избранное');
    });

    test('без productId ничего не рендерит', () => {
        setup();
        const { container } = render(<WishlistButton productId={undefined} />);

        expect(container).toBeEmptyDOMElement();
    });
});
