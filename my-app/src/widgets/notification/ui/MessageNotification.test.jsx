import React from 'react';
import { render, screen } from '@testing-library/react';
import MessageNotification from './MessageNotification';

describe('MessageNotification', () => {
    test('не рендерит ничего если show=false', () => {
        const { container } = render(
            <MessageNotification show={false} message="Тест" />
        );
        expect(container.firstChild).toBeNull();
    });

    test('рендерит сообщение если show=true', () => {
        render(<MessageNotification show={true} message="Товар добавлен!" />);
        expect(screen.getByText('Товар добавлен!')).toBeInTheDocument();
    });

    test('по умолчанию type="success"', () => {
        const { container } = render(
            <MessageNotification show={true} message="Успех" />
        );
        expect(container.querySelector('.success')).toBeInTheDocument();
    });

    test('рендерит иконку ✓ для success', () => {
        render(<MessageNotification show={true} message="Успех" type="success" />);
        expect(screen.getByText('✓')).toBeInTheDocument();
    });

    test('рендерит иконку ⚠ для error', () => {
        render(<MessageNotification show={true} message="Ошибка" type="error" />);
        expect(screen.getByText('⚠')).toBeInTheDocument();
    });

    test('применяет класс "error" для типа error', () => {
        const { container } = render(
            <MessageNotification show={true} message="Ошибка" type="error" />
        );
        expect(container.querySelector('.message-notification')).toHaveClass('error');
    });

    test('применяет класс "success" для типа success', () => {
        const { container } = render(
            <MessageNotification show={true} message="Успех" type="success" />
        );
        expect(container.querySelector('.message-notification')).toHaveClass('success');
    });
});