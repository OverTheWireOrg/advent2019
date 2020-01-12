import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { MatSliderModule } from '@angular/material/slider';

import { AppComponent } from './app.component';
import { GraphComponent } from './graph/graph.component';
import { NodeComponent } from './node/node.component';
import { LinkComponent } from './link/link.component';
import { FlightComponent } from './flight/flight.component';
import { ReplayComponent } from './replay/replay.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule } from '@angular/forms';
import { NgStyle } from '@angular/common';

@NgModule({
  declarations: [
    AppComponent,
    GraphComponent,
    NodeComponent,
    LinkComponent,
    FlightComponent,
    ReplayComponent,
  ],
  imports: [
    BrowserModule,
    MatSliderModule,
    BrowserAnimationsModule,
    FormsModule,
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
