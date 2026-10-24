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

    test('добавляет Content-Type: application/json для запросов с телом', async () => {
        fetch.mockResolvedValueOnce({
            ok: true,
            status: 200,
            json: async () => ({})
        });

        await http.post('/api/test', { a: 1 });

        const callArgs = fetch.mock.calls[0][1];
        expect(callArgs.headers['Content-Type']).toBe('application/json');
    });

    test('GET без тела отправляется без Content-Type (нет лишнего CORS-preflight)', async () => {
        fetch.mockResolvedValueOnce({
            ok: true,
            status: 200,
            json: async () => ({})
        });

        await http('/api/test');

        const callArgs = fetch.mock.calls[0][1];
        expect(callArgs.headers['Content-Type']).toBeUndefined();
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

        // Текст ошибки берётся из ответа сервера
        await expect(http('/api/test')).rejects.toThrow('Bad Request');
    });

    test('выбрасывает ошибку при 5xx статусе', async () => {
        fetch.mockResolvedValueOnce({
            ok: false,
            status: 500,
            json: async () => ({ message: 'Server Error' })
        });

        await expect(http('/api/test')).rejects.toThrow('Server Error');
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

    describe('ошибки сервера', () => {
        test('сообщение из поля error (формат DomainExceptionMiddleware)', async () => {
            fetch.mockResolvedValueOnce({
                ok: false,
                status: 400,
                json: async () => ({ error: 'Недостаточно товара на складе: доступно 2 шт.', type: 'DomainError' })
            });

            const error = await http('/api/test').catch(e => e);

            expect(error.message).toBe('Недостаточно товара на складе: доступно 2 шт.');
            expect(error.status).toBe(400);
            expect(error.data.type).toBe('DomainError');
            expect(error.isConflict).toBe(false);
        });

        test('409 помечается как конфликт', async () => {
            fetch.mockResolvedValueOnce({ ok: false, status: 409, json: async () => ({ error: 'Данные изменились' }) });

            const error = await http('/api/test').catch(e => e);

            expect(error.isConflict).toBe(true);
            expect(error.message).toBe('Данные изменились');
        });

        test('тело не JSON — сообщение HTTP <код>', async () => {
            fetch.mockResolvedValueOnce({ ok: false, status: 502, json: async () => { throw new SyntaxError('bad json'); } });

            await expect(http('/api/test')).rejects.toThrow('HTTP 502');
        });

        test('тело читается через clone(): исходный ответ остаётся непрочитанным', async () => {
            const clone = { json: jest.fn().mockResolvedValue({ error: 'Ошибка' }) };
            const original = { ok: false, status: 400, clone: () => clone, json: jest.fn() };
            fetch.mockResolvedValueOnce(original);

            const error = await http('/api/test').catch(e => e);

            expect(clone.json).toHaveBeenCalled();
            expect(original.json).not.toHaveBeenCalled();
            expect(error.response).toBe(original);
        });

        test('при 401 удаляются только ключи авторизации', async () => {
            delete window.location;
            window.location = { pathname: '/Cart', href: '' };
            localStorage.setItem('token', 't');
            localStorage.setItem('userRole', 'User');
            localStorage.setItem('phones_cache', 'keep-me');

            fetch.mockResolvedValueOnce({ ok: false, status: 401, json: async () => ({}) });

            await expect(http('/api/test')).rejects.toThrow('Unauthorized');

            expect(localStorage.getItem('token')).toBeNull();
            expect(localStorage.getItem('userRole')).toBeNull();
            expect(localStorage.getItem('phones_cache')).toBe('keep-me');
        });
    });
});

