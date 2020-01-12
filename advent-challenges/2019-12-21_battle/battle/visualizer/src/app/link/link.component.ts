import { Component, OnInit, Input } from '@angular/core';
import { InitialInput, Link } from 'src/proto/game_pb';
import { displayScale } from '../consts';

@Component({
  selector: 'app-link',
  templateUrl: './link.component.html',
  styleUrls: ['./link.component.scss']
})
export class LinkComponent implements OnInit {

  @Input()
  public link: Link;

  @Input()
  public initial: InitialInput;
  
  constructor() { }

  ngOnInit() {
  }

  get displayScale() {
    return displayScale;
  }

  get line() {
    let fromStar = this.initial.getStarPositionsList()[this.link.getStarIdA()];
    let toStar = this.initial.getStarPositionsList()[this.link.getStarIdB()];
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
}
