import React from 'react';

const ReviewForm = ({ newReview, reviewSubmitting, onRatingChange, onCommentChange, onSubmit }) => {
    return (
        <div className="add-review">
            <h3>Оставить отзыв</h3>
            <div className="rating-input">
                <span>Ваша оценка: </span>
                {[1, 2, 3, 4, 5].map(star => (
                    <span
                        key={star}
                        className={`star ${newReview.rating >= star ? 'active' : ''}`}
                        onClick={() => onRatingChange(star)}
                    >
                        ★
                    </span>
                ))}
            </div>
            <textarea
                className="review-textarea"
                placeholder="Поделитесь своим мнением..."
                value={newReview.comment}
                onChange={(e) => onCommentChange(e.target.value)}
                rows="4"
            />
            <button
                className="submit-review-btn"
                onClick={onSubmit}
                disabled={reviewSubmitting}
            >
                {reviewSubmitting ? 'Отправка...' : 'Отправить отзыв'}
            </button>
        </div>
    );
};

export default ReviewForm;