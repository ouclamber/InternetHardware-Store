import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import ReviewItem from './ReviewItem';

jest.mock('shared/lib/utils/formatPrice', () => ({
    formatDate: (date) => date ? '07.10.2026' : '—'
}));

const mockReview = {
    id: 1,
    userId: 1,
    userName: 'Иван',
    rating: 5,
    comment: 'Отличный товар, рекомендую!',
    createdAt: '2026-10-07T10:00:00'
};

const defaultProps = {
    review: mockReview,
    isEditing: false,
    editComment: '',
    editRating: 5,
    editSubmitting: false,
    canEditDelete: false,
    onStartEdit: jest.fn(),
    onCancelEdit: jest.fn(),
    onSaveEdit: jest.fn(),
    onChangeComment: jest.fn(),
    onChangeRating: jest.fn(),
    onDelete: jest.fn()
};

describe('ReviewItem', () => {
    beforeEach(() => {
        jest.clearAllMocks();
    });

    test('рендерит имя пользователя', () => {
        render(<ReviewItem {...defaultProps} />);
        expect(screen.getByText('Иван')).toBeInTheDocument();
    });

    test('рендерит комментарий', () => {
        render(<ReviewItem {...defaultProps} />);
        expect(screen.getByText('Отличный товар, рекомендую!')).toBeInTheDocument();
    });

    test('рендерит рейтинг звёздами', () => {
        render(<ReviewItem {...defaultProps} />);
        expect(screen.getByText('★★★★★')).toBeInTheDocument();
    });

    test('рендерит дату', () => {
        render(<ReviewItem {...defaultProps} />);
        expect(screen.getByText('07.10.2026')).toBeInTheDocument();
    });

    test('не показывает кнопки при canEditDelete=false', () => {
        render(<ReviewItem {...defaultProps} canEditDelete={false} />);
        expect(screen.queryByText('Редактировать')).not.toBeInTheDocument();
        expect(screen.queryByText('Удалить')).not.toBeInTheDocument();
    });

    test('показывает кнопки при canEditDelete=true', () => {
        render(<ReviewItem {...defaultProps} canEditDelete={true} />);
        expect(screen.getByText('Редактировать')).toBeInTheDocument();
        expect(screen.getByText('Удалить')).toBeInTheDocument();
    });

    test('клик "Редактировать" вызывает onStartEdit', () => {
        render(<ReviewItem {...defaultProps} canEditDelete={true} />);
        fireEvent.click(screen.getByText('Редактировать'));
        expect(defaultProps.onStartEdit).toHaveBeenCalled();
    });

    test('клик "Удалить" вызывает onDelete', () => {
        render(<ReviewItem {...defaultProps} canEditDelete={true} />);
        fireEvent.click(screen.getByText('Удалить'));
        expect(defaultProps.onDelete).toHaveBeenCalled();
    });

    test('режим редактирования рендерит textarea', () => {
        render(<ReviewItem {...defaultProps} isEditing={true} editComment="Изменённый" />);
        const textarea = screen.getByRole('textbox');
        expect(textarea).toHaveValue('Изменённый');
    });

    test('режим редактирования рендерит кнопку "Сохранить"', () => {
        render(<ReviewItem {...defaultProps} isEditing={true} />);
        expect(screen.getByText('Сохранить')).toBeInTheDocument();
    });

    test('режим редактирования рендерит кнопку "Отмена"', () => {
        render(<ReviewItem {...defaultProps} isEditing={true} />);
        expect(screen.getByText('Отмена')).toBeInTheDocument();
    });

    test('клик "Отмена" вызывает onCancelEdit', () => {
        render(<ReviewItem {...defaultProps} isEditing={true} />);
        fireEvent.click(screen.getByText('Отмена'));
        expect(defaultProps.onCancelEdit).toHaveBeenCalled();
    });

    test('клик "Сохранить" вызывает onSaveEdit', () => {
        render(<ReviewItem {...defaultProps} isEditing={true} />);
        fireEvent.click(screen.getByText('Сохранить'));
        expect(defaultProps.onSaveEdit).toHaveBeenCalled();
    });

    test('кнопка "Сохранить" disabled при editSubmitting', () => {
        render(<ReviewItem {...defaultProps} isEditing={true} editSubmitting={true} />);
        expect(screen.getByText('Сохранение...')).toBeDisabled();
    });
});