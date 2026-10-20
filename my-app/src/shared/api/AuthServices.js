import { http } from './httpClient';
import Configuration from 'shared/config/Configuration';

class AuthServices {
    async SignIn(data) {
        const response = await http.post(Configuration.Auth.SignIn, data);
        const result = await response.json();
        return { data: result };
    }

    async SignUp(data) {
        const response = await http.post(Configuration.Auth.SignUp, data);
        return response;
    }

    async ChangePassword(data) {
        const response = await http.post(Configuration.Auth.ChangePassword, data);
        return response;
    }
}

export default AuthServices;