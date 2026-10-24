import React from 'react';
import { render, screen, waitFor, act } from '@testing-library/react';
import { WishlistProvider, useWishlist } from './WishlistContext';

const mockHttp = jest.fn();
mockHttp.put = jest.fn();
mockHttp.delete = jest.fn();
jest.mock('shared/api/httpClient', () => ({
    get http() { return mockHttp; }
}));

let api;
const Probe = () => {
    api = useWishlist();
    return <div data-testid="count">{api.count}</div>;
};

const login = () => {
    localStorage.setItem('token', 't');
    localStorage.setItem('userId', '1');
};

describe('WishlistContext', () => {
    beforeEach(() => {
        jest.clearAllMocks();
        localStorage.clear();
        api = null;
    });

    test('без провайдера — безопасная заглушка', async () => {
        render(<Probe />);

        expect(api.isInWishlist(1)).toBe(false);
        expect(await api.toggle(1)).toEqual({ success: false });
    });

    test('гость: не ходит на сервер', async () => {
        render(<WishlistProvider><Probe /></WishlistProvider>);

        await act(async () => {});
        expect(mockHttp).not.toHaveBeenCalled();
        expect(screen.getByTestId('count')).toHaveTextContent('0');
    });

    test('гость: toggle просит войти', async () => {
        render(<WishlistProvider><Probe /></WishlistProvider>);

        let result;
        await act(async () => { result = await api.toggle(5); });

        expect(result.success).toBe(false);
        expect(result.message).toMatch(/Войдите/);
    });

    test('загружает Id избранного', async () => {
        login();
        mockHttp.mockResolvedValueOnce({ json: async () => [3, 7] });

        render(<WishlistProvider><Probe /></WishlistProvider>);

        await waitFor(() => expect(screen.getByTestId('count')).toHaveTextContent('2'));
        expect(mockHttp).toHaveBeenCalledWith('/api/Wishlist/ids');
        expect(api.isInWishlist(7)).toBe(true);
        expect(api.isInWishlist(8)).toBe(false);
    });

    test('toggle добавляет (PUT) и удаляет (DELETE)', async () => {
        login();
        mockHttp.mockResolvedValueOnce({ json: async () => [] });
        mockHttp.put.mockResolvedValue({});
        mockHttp.delete.mockResolvedValue({});

        render(<WishlistProvider><Probe /></WishlistProvider>);
        await waitFor(() => expect(mockHttp).toHaveBeenCalled());

        let result;
        await act(async () => { result = await api.toggle(5); });
        expect(result).toEqual({ success: true, added: true });
        expect(mockHttp.put).toHaveBeenCalledWith('/api/Wishlist/5');
        expect(api.isInWishlist(5)).toBe(true);

        await act(async () => { result = await api.toggle(5); });
        expect(result).toEqual({ success: true, added: false });
        expect(mockHttp.delete).toHaveBeenCalledWith('/api/Wishlist/5');
        expect(api.isInWishlist(5)).toBe(false);
    });

    test('ошибка сервера — откат оптимистичного изменения', async () => {
        login();
        mockHttp.mockResolvedValueOnce({ json: async () => [] });
        mockHttp.put.mockRejectedValueOnce(new Error('Товар недоступен'));

        render(<WishlistProvider><Probe /></WishlistProvider>);
        await waitFor(() => expect(mockHttp).toHaveBeenCalled());

        let result;
        await act(async () => { result = await api.toggle(9); });

        expect(result).toEqual({ success: false, message: 'Товар недоступен' });
        expect(api.isInWishlist(9)).toBe(false);
    });

    test('смена authKey (вход) перезагружает избранное', async () => {
        mockHttp.mockResolvedValue({ json: async () => [1] });

        const { rerender } = render(<WishlistProvider authKey=""><Probe /></WishlistProvider>);
        expect(mockHttp).not.toHaveBeenCalled();

        login();
        rerender(<WishlistProvider authKey="t"><Probe /></WishlistProvider>);

        await waitFor(() => expect(screen.getByTestId('count')).toHaveTextContent('1'));
    });
});
