import { userStorage } from './userStorage';

describe('userStorage', () => {
    beforeEach(() => {
        localStorage.clear();
    });

    describe('getUserId', () => {
        test('возвращает null если userId не установлен', () => {
            expect(userStorage.getUserId()).toBeNull();
        });

        test('возвращает userId если он установлен', () => {
            localStorage.setItem('userId', '42');
            expect(userStorage.getUserId()).toBe('42');
        });
    });

    describe('getUserName', () => {
        test('возвращает имя пользователя', () => {
            localStorage.setItem('userName', 'Иван');
            expect(userStorage.getUserName()).toBe('Иван');
        });

        test('возвращает null если имени нет', () => {
            expect(userStorage.getUserName()).toBeNull();
        });
    });

    describe('getUserRole', () => {
        test('возвращает роль пользователя', () => {
            localStorage.setItem('userRole', 'Admin');
            expect(userStorage.getUserRole()).toBe('Admin');
        });
    });

    describe('getToken', () => {
        test('возвращает токен', () => {
            localStorage.setItem('token', 'jwt-token-123');
            expect(userStorage.getToken()).toBe('jwt-token-123');
        });
    });

    describe('setUser', () => {
        test('сохраняет все данные пользователя', () => {
            userStorage.setUser({
                token: 'jwt-token',
                userId: '42',
                userName: 'Иван',
                userRole: 'User'
            });

            expect(localStorage.getItem('token')).toBe('jwt-token');
            expect(localStorage.getItem('userId')).toBe('42');
            expect(localStorage.getItem('userName')).toBe('Иван');
            expect(localStorage.getItem('userRole')).toBe('User');
        });

        test('сохраняет только указанные поля', () => {
            userStorage.setUser({ token: 'jwt-token' });

            expect(localStorage.getItem('token')).toBe('jwt-token');
            expect(localStorage.getItem('userId')).toBeNull();
        });
    });

    describe('clear', () => {
        test('удаляет все данные пользователя', () => {
            localStorage.setItem('token', 'jwt-token');
            localStorage.setItem('userId', '42');
            localStorage.setItem('userName', 'Иван');
            localStorage.setItem('userRole', 'User');
            localStorage.setItem('needRefreshProfile', 'true');

            userStorage.clear();

            expect(localStorage.getItem('token')).toBeNull();
            expect(localStorage.getItem('userId')).toBeNull();
            expect(localStorage.getItem('userName')).toBeNull();
            expect(localStorage.getItem('userRole')).toBeNull();
            expect(localStorage.getItem('needRefreshProfile')).toBeNull();
        });
    });

    describe('isAuthenticated', () => {
        test('возвращает false если нет данных', () => {
            expect(userStorage.isAuthenticated()).toBe(false);
        });

        test('возвращает false если только userId', () => {
            localStorage.setItem('userId', '42');
            expect(userStorage.isAuthenticated()).toBe(false);
        });

        test('возвращает false если только token', () => {
            localStorage.setItem('token', 'jwt-token');
            expect(userStorage.isAuthenticated()).toBe(false);
        });

        test('возвращает true если есть userId и token', () => {
            localStorage.setItem('userId', '42');
            localStorage.setItem('token', 'jwt-token');
            expect(userStorage.isAuthenticated()).toBe(true);
        });
    });

    describe('isAdmin', () => {
        test('возвращает false для обычного пользователя', () => {
            localStorage.setItem('userRole', 'User');
            expect(userStorage.isAdmin()).toBe(false);
        });

        test('возвращает true для админа', () => {
            localStorage.setItem('userRole', 'Admin');
            expect(userStorage.isAdmin()).toBe(true);
        });

        test('возвращает false если роль не установлена', () => {
            expect(userStorage.isAdmin()).toBe(false);
        });
    });
});