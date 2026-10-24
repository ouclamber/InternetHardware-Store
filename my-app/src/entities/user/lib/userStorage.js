export const userStorage = {
    getUserId: () => localStorage.getItem('userId'),
    getUserName: () => localStorage.getItem('userName'),
    getUserRole: () => localStorage.getItem('userRole'),
    getToken: () => localStorage.getItem('token'),

    setUser: ({ token, userId, userName, userRole }) => {
        if (token) localStorage.setItem('token', token);
        if (userId) localStorage.setItem('userId', userId);
        if (userName) localStorage.setItem('userName', userName);
        if (userRole) localStorage.setItem('userRole', userRole);
    },

    clear: () => {
        localStorage.removeItem('userId');
        localStorage.removeItem('userName');
        localStorage.removeItem('userRole');
        localStorage.removeItem('token');
        localStorage.removeItem('needRefreshProfile');
    },

    isAuthenticated: () => {
        return !!localStorage.getItem('userId') && !!localStorage.getItem('token');
    },

    isAdmin: () => localStorage.getItem('userRole') === 'Admin'
};