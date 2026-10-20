import { http } from './httpClient';
import { API_URL } from './baseUrl';

global.fetch = jest.fn();

describe('httpClient', () => {
    beforeEach(() => {
        fetch.mockClear();
        localStorage.clear();
    });

    test('подставляет API_URL если endpoint начинается с /', async () => {
        fetch.mockResolvedValueOnce({
            ok: true,
            status: 200,
            json: async () => ({ data: 'test' })
        });

        await http('/api/test');

        expect(fetch).toHaveBeenCalledWith(
            `${API_URL}/api/test`,
            expect.any(Object)
        );
    });

    test('не подставляет API_URL если endpoint начинается с http', async () => {
        fetch.mockResolvedValueOnce({
            ok: true,
            status: 200,
            json: async () => ({ data: 'test' })
        });

        await http('https://external-api.com/data');

        expect(fetch).toHaveBeenCalledWith(
            'https://external-api.com/data',
            expect.any(Object)
        );
    });

    test('добавляет Content-Type: application/json', async () => {
        fetch.mockResolvedValueOnce({
            ok: true,
            status: 200,
            json: async () => ({})
        });

        await http('/api/test');

        const callArgs = fetch.mock.calls[0][1];
        expect(callArgs.headers['Content-Type']).toBe('application/json');
    });

    test('добавляет Authorization если есть токен', async () => {
        localStorage.setItem('token', 'jwt-token-123');

        fetch.mockResolvedValueOnce({
            ok: true,
            status: 200,
            json: async () => ({})
        });

        await http('/api/test');

        const callArgs = fetch.mock.calls[0][1];
        expect(callArgs.headers['Authorization']).toBe('Bearer jwt-token-123');
    });

    test('не добавляет Authorization если нет токена', async () => {
        fetch.mockResolvedValueOnce({
            ok: true,
            status: 200,
            json: async () => ({})
        });

        await http('/api/test');

        const callArgs = fetch.mock.calls[0][1];
        expect(callArgs.headers['Authorization']).toBeUndefined();
    });

    test('выбрасывает ошибку при 4xx статусе', async () => {
        fetch.mockResolvedValueOnce({
            ok: false,
            status: 400,
            json: async () => ({ message: 'Bad Request' })
        });

        await expect(http('/api/test')).rejects.toThrow('HTTP 400');
    });

    test('выбрасывает ошибку при 5xx статусе', async () => {
        fetch.mockResolvedValueOnce({
            ok: false,
            status: 500,
            json: async () => ({ message: 'Server Error' })
        });

        await expect(http('/api/test')).rejects.toThrow('HTTP 500');
    });

    test('при 401 очищает localStorage и редиректит', async () => {
        delete window.location;
        window.location = { pathname: '/some-page', href: '' };

        localStorage.setItem('token', 'jwt-token');
        localStorage.setItem('userId', '42');

        fetch.mockResolvedValueOnce({
            ok: false,
            status: 401,
            json: async () => ({ message: 'Unauthorized' })
        });

        await expect(http('/api/test')).rejects.toThrow('Unauthorized');

        expect(localStorage.getItem('token')).toBeNull();
        expect(localStorage.getItem('userId')).toBeNull();
        expect(window.location.href).toBe('/SignIn');
    });

    test('при 401 НЕ редиректит если уже на /SignIn', async () => {
        delete window.location;
        window.location = { pathname: '/SignIn', href: '' };

        fetch.mockResolvedValueOnce({
            ok: false,
            status: 401,
            json: async () => ({ message: 'Unauthorized' })
        });

        await expect(http('/api/test')).rejects.toThrow('Unauthorized');

        expect(window.location.href).toBe('');
    });

    describe('http.get', () => {
        test('делает GET-запрос', async () => {
            fetch.mockResolvedValueOnce({
                ok: true,
                status: 200,
                json: async () => ({})
            });

            await http.get('/api/test');

            const callArgs = fetch.mock.calls[0][1];
            expect(callArgs.method).toBe('GET');
        });
    });

    describe('http.post', () => {
        test('делает POST-запрос с JSON body', async () => {
            fetch.mockResolvedValueOnce({
                ok: true,
                status: 200,
                json: async () => ({})
            });

            const body = { name: 'Test', value: 42 };
            await http.post('/api/test', body);

            const callArgs = fetch.mock.calls[0][1];
            expect(callArgs.method).toBe('POST');
            expect(callArgs.body).toBe(JSON.stringify(body));
        });
    });

    describe('http.put', () => {
        test('делает PUT-запрос с JSON body', async () => {
            fetch.mockResolvedValueOnce({
                ok: true,
                status: 200,
                json: async () => ({})
            });

            await http.put('/api/test/1', { name: 'Updated' });

            const callArgs = fetch.mock.calls[0][1];
            expect(callArgs.method).toBe('PUT');
        });
    });

    describe('http.delete', () => {
        test('делает DELETE-запрос', async () => {
            fetch.mockResolvedValueOnce({
                ok: true,
                status: 200,
                json: async () => ({})
            });

            await http.delete('/api/test/1');

            const callArgs = fetch.mock.calls[0][1];
            expect(callArgs.method).toBe('DELETE');
        });
    });
});