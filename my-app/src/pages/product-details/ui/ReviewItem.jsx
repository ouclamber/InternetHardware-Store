import React from 'react';
import './ReviewItem.css';
import { formatDate } from 'shared/lib/utils/formatPrice';

const ReviewItem = ({
    review,
    isEditing,
    editComment,
    editRating,
    editSubmitting,
    canEditDelete,
    onStartEdit,
    onCancelEdit,
    onSaveEdit,
    onChangeComment,
    onChangeRating,
    onDelete
}) => {
    if (isEditing) {
        return (
            <div className="review-item editing">
                <div className="review-header">
                    <span className="review-author">{review.userName}</span>
                    <div className="rating-input edit-rating">
                        {[1, 2, 3, 4, 5].map(star => (
                            <span
                                key={star}
                                className={`star ${editRating >= star ? 'active' : ''}`}
                                onClick={() => onChangeRating(star)}
                            >
                                ★
                            </span>
                        ))}
                    </div>
                </div>
                <textarea
                    className="edit-review-textarea"
                    value={editComment}
                    onChange={(e) => onChangeComment(e.target.value)}
                    rows="3"
                />
                <div className="edit-review-actions">
                    <button
                        className="save-edit-btn"
                        onClick={onSaveEdit}
                        disabled={editSubmitting}
                    >
                        {editSubmitting ? 'Сохранение...' : 'Сохранить'}
                    </button>
                    <button className="cancel-edit-btn" onClick={onCancelEdit}>
                        Отмена
                    </button>
                </div>
            </div>
        );
    }

    return (
        <div className="review-item">
            <div className="review-header">
                <span className="review-author">{review.userName}</span>
                <span className="review-rating">
                    {'★'.repeat(review.rating)}{'☆'.repeat(5 - review.rating)}
                </span>
                <span className="review-date">{formatDate(review.createdAt)}</span>
                {canEditDelete && (
                    <div className="review-actions">
                        <button
                            className="edit-review-btn"
                            onClick={onStartEdit}
                            title="Редактировать"
                        >
                            Редактировать
                        </button>
                        <button
                            className="delete-review-btn"
                            onClick={onDelete}
                            title="Удалить"
                        >
                            Удалить
                        </button>
                    </div>
                )}
            </div>
            <p className="review-comment">{review.comment}</p>
        </div>
    );
};

export default ReviewItem;