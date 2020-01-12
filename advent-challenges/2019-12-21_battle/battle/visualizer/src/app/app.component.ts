import { Component } from '@angular/core';
import * as JSZip from 'jszip';
import { GameRecord, TurnInput, Star, StarPosition, Flight, Link } from 'src/proto/game_pb';
import { displayScale } from './consts';
import { Settings } from './settings';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  public record: GameRecord = new GameRecord();

  _turn = 0;
  turnData: TurnInput = new TurnInput();
  stars: OneStar[] = [];
  flights: Flight[] = [];
  links: Link[] = [];
  settings: Settings = new Settings();

  get turn() {
    return this._turn;
  }

  set turn(t: number) {
    this._turn = t;
    this.recomputeTurnData();
  }

  get numTurns() {
    return this.record.getTurnsList().length;
  }

  get displayScale() {
    return displayScale;
  }

  recomputeTurnData() {
    this.stars.length = 0;
    for (let position of this.record.getInitialConfiguration().getStarPositionsList()) {
      this.stars.push(new OneStar(null, position));
    }
    for (let star of this.stars) {
      star.star = new Star();
      star.star.setOwner(-2);
    }
    this.turnData = this.record.getTurnsList()[this.turn];
    for (let star of this.turnData.getStarsList()) {
      this.stars[star.getId()].star = star;
    }

    this.flights.length = 0;
    for (let flight of this.turnData.getFlightList()) {
      this.flights.push(flight);
    }
    this.links.length = 0;
    for (let link of this.turnData.getLinkList()) {
      this.links.push(link);
    }
  }

  async handleFileInput(files: FileList) {
    let zip = await JSZip.loadAsync(files[0]);
    zip.forEach(async (relativePath, zipEntry) => {
      let data = await zipEntry.async('uint8array');
      this.record = GameRecord.deserializeBinary(data);
      this.turn = 0;
    });
  }

}

class OneStar {
  public constructor(public star: Star | null, public position: StarPosition) { }
}
