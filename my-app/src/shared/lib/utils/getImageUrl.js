import { API_URL } from 'shared/api/baseUrl';

export const getImageUrl = (url) => {
    if (!url || url === 'string') return null;  
    if (url.startsWith('http')) return url;
    return `${API_URL}${url.startsWith('/') ? '' : '/'}${url}`;
};