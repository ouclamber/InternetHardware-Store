import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import Pagination from './Pagination';

describe('Pagination', () => {
    test('не рендерится, если totalPages <= 1', () => {
        const { container } = render(
            <Pagination currentPage={1} totalPages={1} onPageChange={() => {}} />
        );
        expect(container.firstChild).toBeNull();
    });

    test('рендерит кнопки страниц', () => {
        render(<Pagination currentPage={1} totalPages={5} onPageChange={() => {}} />);
        expect(screen.getByText('1')).toBeInTheDocument();
        expect(screen.getByText('5')).toBeInTheDocument();
    });

    test('вызывает onPageChange при клике на страницу', () => {
        const onPageChange = jest.fn();
        render(<Pagination currentPage={1} totalPages={5} onPageChange={onPageChange} />);
        fireEvent.click(screen.getByText('3'));
        expect(onPageChange).toHaveBeenCalledWith(3);
    });

    test('кнопка "Назад" disabled на первой странице', () => {
        render(<Pagination currentPage={1} totalPages={5} onPageChange={() => {}} />);
        expect(screen.getByText('Назад')).toBeDisabled();
    });

    test('кнопка "Вперёд" disabled на последней странице', () => {
        render(<Pagination currentPage={5} totalPages={5} onPageChange={() => {}} />);
        expect(screen.getByText('Вперёд')).toBeDisabled();
    });

    test('клик "Назад" вызывает onPageChange(currentPage - 1)', () => {
        const onPageChange = jest.fn();
        render(<Pagination currentPage={3} totalPages={5} onPageChange={onPageChange} />);
        fireEvent.click(screen.getByText('Назад'));
        expect(onPageChange).toHaveBeenCalledWith(2);
    });

    test('клик "Вперёд" вызывает onPageChange(currentPage + 1)', () => {
        const onPageChange = jest.fn();
        render(<Pagination currentPage={3} totalPages={5} onPageChange={onPageChange} />);
        fireEvent.click(screen.getByText('Вперёд'));
        expect(onPageChange).toHaveBeenCalledWith(4);
    });

    test('активная страница имеет класс active', () => {
        render(<Pagination currentPage={3} totalPages={5} onPageChange={() => {}} />);
        expect(screen.getByText('3')).toHaveClass('active');
    });

    test('показывает многоточие для больших диапазонов', () => {
        render(<Pagination currentPage={10} totalPages={20} onPageChange={() => {}} />);
        expect(screen.getAllByText('...').length).toBeGreaterThan(0);
    });

    test('клик на "1" при currentPage=10', () => {
        const onPageChange = jest.fn();
        render(<Pagination currentPage={10} totalPages={20} onPageChange={onPageChange} />);
        fireEvent.click(screen.getByText('1'));
        expect(onPageChange).toHaveBeenCalledWith(1);
    });
});