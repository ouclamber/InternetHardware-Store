import React, { Component } from 'react';
import '../../sign-up/ui/SignUp.css';
import { TextField, Radio, RadioGroup, FormControlLabel, Button, Alert, CircularProgress, Snackbar } from '@mui/material';
import { withNavigate } from 'shared/lib/hoc/withNavigate';
import AuthServices from 'shared/api/AuthServices';
import { userStorage } from 'entities/user/lib/userStorage';

const authService = new AuthServices();

class SignIn extends Component {
    constructor(props) {
        super(props);
        this.state = {
            UserName: '', Password: '', RoleValue: 'User',
            UserNameFlag: false, PasswordFlag: false,
            UserNameError: '', PasswordError: '', GeneralError: '',
            Loading: false, SnackbarOpen: false, SnackbarMessage: '', SnackbarSeverity: 'success'
        };
    }

    handleValues = (e) => {
        const { name, value } = e.target;
        this.setState({ [name]: value, GeneralError: '', [name + 'Flag']: false, [name + 'Error']: '' });
    }

    handleChangeRole = (e) => {
        this.setState({ RoleValue: e.target.value });
    }

    handleSignUp = () => this.props.navigate('/SignUp');
    handleCloseSnackbar = () => this.setState({ SnackbarOpen: false });

    handleSubmit = async () => {
        this.setState({ Loading: true, GeneralError: '', UserNameFlag: false, PasswordFlag: false });
        if (!this.state.UserName.trim()) {
            this.setState({ UserNameFlag: true, UserNameError: 'Введите имя пользователя', Loading: false });
            return;
        }
        if (!this.state.Password) {
            this.setState({ PasswordFlag: true, PasswordError: 'Введите пароль', Loading: false });
            return;
        }
        const data = { UserName: this.state.UserName.trim(), Password: this.state.Password, Role: this.state.RoleValue };
        try {
            const response = await authService.SignIn(data);
            if (response?.data?.isSuccess && response?.data?.token) {
                userStorage.clear();
                userStorage.setUser({
                    token: response.data.token,
                    userId: response.data.userId,
                    userName: response.data.userName,
                    userRole: response.data.role
                });
                this.setState({
                    Loading: false, SnackbarOpen: true,
                    SnackbarMessage: `Добро пожаловать, ${response.data.userName || 'пользователь'}!`,
                    SnackbarSeverity: 'success'
                });
                setTimeout(() => this.props.navigate('/HomePage'), 2000);
            } else {
                this.setState({ GeneralError: response?.data?.message || 'Ошибка входа', Loading: false });
            }
        } catch (error) {
            let errorMessage = 'Ошибка при входе';
            if (error.response?.data) {
                errorMessage = error.response.data.message || error.response.data.title || 'Неверные учетные данные';
                if (error.response.data.errors) {
                    const errors = Object.values(error.response.data.errors).flat();
                    if (errors.length) errorMessage = errors[0];
                }
            } else if (error.request) { errorMessage = 'Нет связи с сервером'; }
            this.setState({ GeneralError: errorMessage, Loading: false });
        }
    }

    render() {
        const { Loading, GeneralError, UserNameError, PasswordError, UserNameFlag, PasswordFlag, SnackbarOpen, SnackbarMessage, SnackbarSeverity } = this.state;

        return (
            <div className='SignUp-Container'>
                <Snackbar open={SnackbarOpen} autoHideDuration={3000} onClose={this.handleCloseSnackbar}
                    anchorOrigin={{ vertical: 'top', horizontal: 'center' }}>
                    <Alert onClose={this.handleCloseSnackbar} severity={SnackbarSeverity} sx={{ width: '100%', fontSize: '16px' }}>
                        {SnackbarMessage}
                    </Alert>
                </Snackbar>

                <div className='SignUp-SubContainer'>
                    <div className='Header'>SignIn</div>
                    <div className='Body'>
                        <form className='form'>
                            {GeneralError && <Alert severity="error" sx={{ mb: 2, width: '300px' }}>{GeneralError}</Alert>}
                            <TextField error={UserNameFlag} className='TextField' name='UserName' label="UserName"
                                variant="outlined" size='small' value={this.state.UserName}
                                onChange={this.handleValues} helperText={UserNameError} disabled={Loading} />
                            <TextField error={PasswordFlag} className='TextField' name='Password' label="Password"
                                variant="outlined" size='small' type="password" value={this.state.Password}
                                onChange={this.handleValues} helperText={PasswordError} disabled={Loading} />
                            <RadioGroup className='Roles' name="Role" value={this.state.RoleValue} onChange={this.handleChangeRole}>
                                <FormControlLabel className='RoleValue' value="Admin" control={<Radio />} label="Admin" />
                                <FormControlLabel className='RoleValue' value="User" control={<Radio />} label="User" />
                            </RadioGroup>
                        </form>
                    </div>
                    <div className='buttons'>
                        <Button className='Btn' color='primary' onClick={this.handleSignUp} disabled={Loading}>
                            Create New Account
                        </Button>
                        <Button className='Btn' variant="contained" color='primary' onClick={this.handleSubmit} disabled={Loading}>
                            {Loading ? <CircularProgress size={24} /> : 'Sign In'}
                        </Button>
                    </div>
                </div>
            </div>
        );
    }
}

export default withNavigate(SignIn);