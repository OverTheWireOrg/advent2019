import { Component, OnInit, Input } from '@angular/core';
import { InitialInput, Flight } from 'src/proto/game_pb';
import { displayScale } from '../consts';
import { DomSanitizer } from '@angular/platform-browser';

@Component({
  selector: 'app-flight',
  templateUrl: './flight.component.html',
  styleUrls: ['./flight.component.scss']
})
export class FlightComponent implements OnInit {

  @Input()
  public flight: Flight;

  @Input()
  public initial: InitialInput;

  constructor(private sanitizer:DomSanitizer) {}

  get position() {
    let fromStar = this.initial.getStarPositionsList()[this.flight.getFromStarId()];
    let toStar = this.initial.getStarPositionsList()[this.flight.getToStarId()];
    let dist = Math.hypot(fromStar.getX() - toStar.getX(), fromStar.getY() - toStar.getY());
    let turns = Math.ceil(dist / 10);
    let progress = (turns - this.flight.getTurnsToArrival()) / turns;
    let posX = fromStar.getX() + progress * (toStar.getX() - fromStar.getX());
    let posY = fromStar.getY() + progress * (toStar.getY() - fromStar.getY());
    return { x: posX, y: posY };
  }

  get rotation() {
    let fromStar = this.initial.getStarPositionsList()[this.flight.getFromStarId()];
    let toStar = this.initial.getStarPositionsList()[this.flight.getToStarId()];
    let angle = Math.atan2(toStar.getY() - fromStar.getY(), toStar.getX() - fromStar.getX());
    return angle;
  }

  get rotationStyle() {
    return this.sanitizer.bypassSecurityTrustStyle('transform: rotate(' + this.rotation + 'rad)');
  }

  get owner() {
    return this.flight.getOwner();
  }

  get affiliation() {
    return this.owner < 10 ? 0 : 1;
  }

  get displayScale() {
    return displayScale;
  }

  get line() {
    let fromStar = this.initial.getStarPositionsList()[this.flight.getFromStarId()];
    let toStar = this.initial.getStarPositionsList()[this.flight.getToStarId()];
    let minX = Math.min(fromStar.getX(), toStar.getX());
    let minY = Math.min(fromStar.getY(), toStar.getY());
    let maxX = Math.max(fromStar.getX(), toStar.getX());
    let maxY = Math.max(fromStar.getY(), toStar.getY());
    return {
      width: maxX - minX,
      height: maxY - minY,
      left: minX,
      top: minY,
      from: {
        x: fromStar.getX() - minX,
        y: fromStar.getY() - minY,
      },
      to: {
        x: toStar.getX() - minX,
        y: toStar.getY() - minY,
      }
    };
  }

  ngOnInit() {
  }

}
