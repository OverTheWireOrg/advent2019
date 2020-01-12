import { Component, OnInit, Input } from '@angular/core';
import { Star, StarPosition } from 'src/proto/game_pb';
import { displayScale } from '../consts';
import { Settings } from '../settings';

@Component({
  selector: 'app-node',
  templateUrl: './node.component.html',
  styleUrls: ['./node.component.scss']
})
export class NodeComponent implements OnInit {
  @Input()
  public star: Star;

  @Input()
  public position: StarPosition;

  @Input()
  public settings: Settings;

  get owner() {
    return this.star.getOwner() < 0 ? '' : this.star.getOwner();
  }

  get affiliation() {
    return this.star.getOwner() < 0 ? this.star.getOwner() : this.star.getOwner() < 10 ? 0 : 1;
  }

  get displayScale() {
    return displayScale;
  }

  constructor() { }

  ngOnInit() {
  }

}
