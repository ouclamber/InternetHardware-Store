import { useEffect, useState } from 'react';

/**
 * Значение, которое обновляется только после паузы ввода.
 * Нужен, чтобы поле «Цена от» не отправляло запрос на сервер на каждое нажатие клавиши.
 */
export const useDebouncedValue = (value, delay = 400) => {
    const [debounced, setDebounced] = useState(value);

    useEffect(() => {
        const timer = setTimeout(() => setDebounced(value), delay);
        return () => clearTimeout(timer);
    }, [value, delay]);

    return debounced;
};
