/**
 * Лёгкие элементы форм вместо @mui/material.
 *
 * MUI использовался только на страницах входа и регистрации, но тянул ~200 КБ JS
 * (@mui/material + @emotion + react-transition-group) — и это первая страница,
 * которую видит неавторизованный пользователь. API повторяет используемую часть MUI,
 * поэтому страницы почти не изменились.
 */
import React, { useEffect, useId } from 'react';
import './Form.css';

const cx = (...classes) => classes.filter(Boolean).join(' ');

export const TextField = ({
    label,
    name,
    value,
    onChange,
    type = 'text',
    error = false,
    helperText,
    disabled = false,
    className,
    autoComplete,
    // совместимость с MUI-пропсами, которые больше не нужны
    variant: _variant,
    size: _size,
    ...rest
}) => {
    const id = useId();
    const helperId = helperText ? `${id}-helper` : undefined;

    return (
        <div className={cx('form-field', error && 'form-field--error', disabled && 'form-field--disabled', className)}>
            <input
                id={id}
                className="form-field__input"
                name={name}
                type={type}
                value={value}
                onChange={onChange}
                disabled={disabled}
                autoComplete={autoComplete}
                placeholder=" "
                aria-invalid={error || undefined}
                aria-describedby={helperId}
                {...rest}
            />
            <label htmlFor={id} className="form-field__label">{label}</label>
            {helperText && (
                <p id={helperId} className="form-field__helper">{helperText}</p>
            )}
        </div>
    );
};

export const RadioGroup = ({ name, value, onChange, className, children }) => (
    <div role="radiogroup" className={cx('form-radio-group', className)}>
        {React.Children.map(children, child =>
            React.isValidElement(child)
                ? React.cloneElement(child, {
                    name,
                    checked: child.props.value === value,
                    onChange
                })
                : child
        )}
    </div>
);

/** Совместимость с MUI: значение `control` игнорируется, всегда рисуется radio. */
export const Radio = () => null;

export const FormControlLabel = ({ value, label, name, checked = false, onChange, className, control: _control }) => (
    <label className={cx('form-choice', checked && 'form-choice--checked', className)}>
        <input
            type="radio"
            className="form-choice__input"
            name={name}
            value={value}
            checked={checked}
            onChange={onChange}
        />
        <span className="form-choice__label">{label}</span>
    </label>
);

export const Button = ({
    children,
    onClick,
    disabled = false,
    variant = 'text',
    type = 'button',
    className,
    color: _color,
    ...rest
}) => (
    <button
        type={type}
        className={cx('form-btn', `form-btn--${variant}`, className)}
        onClick={onClick}
        disabled={disabled}
        {...rest}
    >
        {children}
    </button>
);

export const Alert = ({ severity = 'info', onClose, children, className, sx: _sx }) => (
    <div role="alert" className={cx('form-alert', `form-alert--${severity}`, className)}>
        <span className="form-alert__message">{children}</span>
        {onClose && (
            <button type="button" className="form-alert__close" onClick={onClose} aria-label="Закрыть">×</button>
        )}
    </div>
);

export const CircularProgress = ({ size = 24 }) => (
    <span
        className="form-progress"
        role="progressbar"
        aria-label="Загрузка"
        style={{ width: size, height: size }}
    />
);

export const Snackbar = ({ open, autoHideDuration, onClose, children, anchorOrigin: _anchorOrigin }) => {
    useEffect(() => {
        if (!open || !autoHideDuration || !onClose) return undefined;
        const timer = setTimeout(onClose, autoHideDuration);
        return () => clearTimeout(timer);
    }, [open, autoHideDuration, onClose]);

    if (!open) return null;

    return <div className="form-snackbar">{children}</div>;
};
