import { Component } from '@angular/core';
import { WorkspaceLayout } from '../../shared/components/workspace-layout/workspace-layout';
import { MetricCard } from '../../shared/components/metric-card/metric-card';
import { PrimaryButton } from '../../shared/components/primary-button/primary-button';
@Component({selector:'app-dashboard-page',standalone:true,imports:[WorkspaceLayout,MetricCard,PrimaryButton],templateUrl:'./dashboard-page.html',styleUrl:'./dashboard-page.scss'}) export class DashboardPage {}
