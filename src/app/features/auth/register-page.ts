import { Component } from '@angular/core'; import { RouterLink } from '@angular/router'; import { PrimaryButton } from '../../shared/components/primary-button/primary-button';
@Component({selector:'app-register-page',standalone:true,imports:[RouterLink,PrimaryButton],templateUrl:'./register-page.html',styleUrl:'./auth-page.scss'}) export class RegisterPage {}
