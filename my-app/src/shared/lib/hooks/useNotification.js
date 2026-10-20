import { useState, useRef, useEffect } from 'react';

export const useNotification = (duration = 3000) => {
    const [notification, setNotification] = useState({
        show: false,
        message: '',
        type: 'success',
        productName: ''
    });
    const timeoutRef = useRef(null);

    useEffect(() => {
        return () => {
            if (timeoutRef.current) clearTimeout(timeoutRef.current);
        };
    }, []);

    const showNotification = (message, type = 'success', productName = '') => {
        if (timeoutRef.current) clearTimeout(timeoutRef.current);
        setNotification({ show: true, message, type, productName });
        timeoutRef.current = setTimeout(() => {
            setNotification({ show: false, message: '', type: 'success', productName: '' });
        }, duration);
    };

    const hideNotification = () => {
        if (timeoutRef.current) clearTimeout(timeoutRef.current);
        setNotification({ show: false, message: '', type: 'success', productName: '' });
    };

    return { notification, showNotification, hideNotification };
};