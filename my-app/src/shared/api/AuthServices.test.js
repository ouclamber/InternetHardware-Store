import AuthServices from './AuthServices';
import { http } from './httpClient';

jest.mock('./httpClient', () => ({
    http: Object.assign(jest.fn(), {
        post: jest.fn()
    })
}));

describe('AuthServices', () => {
    let authService;

    beforeEach(() => {
        authService = new AuthServices();
        jest.clearAllMocks();
    });

    test('SignIn вызывает http.post с правильными аргументами', async () => {
        http.post.mockResolvedValueOnce({
            ok: true,
            json: async () => ({ token: 'abc', userId: 1 })
        });

        const result = await authService.SignIn({
            UserName: 'user',
            Password: 'pass',
            Role: 'User'
        });

        expect(http.post).toHaveBeenCalledWith(
            '/api/Auth/signin',
            { UserName: 'user', Password: 'pass', Role: 'User' }
        );
        expect(result.data).toEqual({ token: 'abc', userId: 1 });
    });

    test('SignIn возвращает { data }', async () => {
        http.post.mockResolvedValueOnce({
            ok: true,
            json: async () => ({ token: 'xyz' })
        });

        const result = await authService.SignIn({});
        expect(result).toHaveProperty('data');
    });

    test('SignUp вызывает http.post на /api/Auth/signup', async () => {
        http.post.mockResolvedValueOnce({ ok: true });

        await authService.SignUp({ UserName: 'new' });

        expect(http.post).toHaveBeenCalledWith(
            '/api/Auth/signup',
            { UserName: 'new' }
        );
    });

    test('ChangePassword вызывает http.post на /api/Auth/change-password', async () => {
        http.post.mockResolvedValueOnce({ ok: true });

        await authService.ChangePassword({
            OldPassword: 'old',
            NewPassword: 'new'
        });

        expect(http.post).toHaveBeenCalledWith(
            '/api/Auth/change-password',
            { OldPassword: 'old', NewPassword: 'new' }
        );
    });
});