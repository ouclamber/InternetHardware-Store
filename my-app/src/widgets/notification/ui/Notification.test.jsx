import React from 'react';
import { render, screen } from '@testing-library/react';
import Notification from './Notification';

describe('Notification', () => {
    test('не рендерит ничего если show=false', () => {
        const { container } = render(
            <Notification show={false} productName="Ноутбук" />
        );
        expect(container.firstChild).toBeNull();
    });

    test('рендерит уведомление если show=true', () => {
        render(<Notification show={true} productName="Ноутбук" />);
        expect(screen.getByText('Ноутбук')).toBeInTheDocument();
    });

    test('рендерит текст по умолчанию "Товар добавлен в корзину!"', () => {
        render(<Notification show={true} productName="Ноутбук" />);
        expect(screen.getByText('Товар добавлен в корзину!')).toBeInTheDocument();
    });

    test('рендерит кастомное сообщение', () => {
        render(
            <Notification
                show={true}
                productName="Ноутбук"
                message="Товар успешно добавлен"
            />
        );
        expect(screen.getByText('Товар успешно добавлен')).toBeInTheDocument();
    });

    test('рендерит название товара', () => {
        render(<Notification show={true} productName="ASUS VivoBook 15" />);
        expect(screen.getByText('ASUS VivoBook 15')).toBeInTheDocument();
    });
});