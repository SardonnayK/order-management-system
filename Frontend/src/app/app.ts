import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { HlmButton } from '@spartan-ng/helm/button';

@Component({
  imports: [RouterOutlet, RouterLink, RouterLinkActive, HlmButton],
  selector: 'app-root',
  templateUrl: './app.html',
})
export class App {}
