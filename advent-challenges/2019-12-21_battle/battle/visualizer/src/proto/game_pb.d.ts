// package: Game
// file: game.proto

import * as jspb from "google-protobuf";

export class InitialInput extends jspb.Message {
  clearStarPositionsList(): void;
  getStarPositionsList(): Array<StarPosition>;
  setStarPositionsList(value: Array<StarPosition>): void;
  addStarPositions(value?: StarPosition, index?: number): StarPosition;

  serializeBinary(): Uint8Array;
  toObject(includeInstance?: boolean): InitialInput.AsObject;
  static toObject(includeInstance: boolean, msg: InitialInput): InitialInput.AsObject;
  static extensions: {[key: number]: jspb.ExtensionFieldInfo<jspb.Message>};
  static extensionsBinary: {[key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message>};
  static serializeBinaryToWriter(message: InitialInput, writer: jspb.BinaryWriter): void;
  static deserializeBinary(bytes: Uint8Array): InitialInput;
  static deserializeBinaryFromReader(message: InitialInput, reader: jspb.BinaryReader): InitialInput;
}

export namespace InitialInput {
  export type AsObject = {
    starPositionsList: Array<StarPosition.AsObject>,
  }
}

export class TurnInput extends jspb.Message {
  clearStarsList(): void;
  getStarsList(): Array<Star>;
  setStarsList(value: Array<Star>): void;
  addStars(value?: Star, index?: number): Star;

  clearLinkList(): void;
  getLinkList(): Array<Link>;
  setLinkList(value: Array<Link>): void;
  addLink(value?: Link, index?: number): Link;

  clearFlightList(): void;
  getFlightList(): Array<Flight>;
  setFlightList(value: Array<Flight>): void;
  addFlight(value?: Flight, index?: number): Flight;

  serializeBinary(): Uint8Array;
  toObject(includeInstance?: boolean): TurnInput.AsObject;
  static toObject(includeInstance: boolean, msg: TurnInput): TurnInput.AsObject;
  static extensions: {[key: number]: jspb.ExtensionFieldInfo<jspb.Message>};
  static extensionsBinary: {[key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message>};
  static serializeBinaryToWriter(message: TurnInput, writer: jspb.BinaryWriter): void;
  static deserializeBinary(bytes: Uint8Array): TurnInput;
  static deserializeBinaryFromReader(message: TurnInput, reader: jspb.BinaryReader): TurnInput;
}

export namespace TurnInput {
  export type AsObject = {
    starsList: Array<Star.AsObject>,
    linkList: Array<Link.AsObject>,
    flightList: Array<Flight.AsObject>,
  }
}

export class TurnOutput extends jspb.Message {
  clearFlyList(): void;
  getFlyList(): Array<Fly>;
  setFlyList(value: Array<Fly>): void;
  addFly(value?: Fly, index?: number): Fly;

  serializeBinary(): Uint8Array;
  toObject(includeInstance?: boolean): TurnOutput.AsObject;
  static toObject(includeInstance: boolean, msg: TurnOutput): TurnOutput.AsObject;
  static extensions: {[key: number]: jspb.ExtensionFieldInfo<jspb.Message>};
  static extensionsBinary: {[key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message>};
  static serializeBinaryToWriter(message: TurnOutput, writer: jspb.BinaryWriter): void;
  static deserializeBinary(bytes: Uint8Array): TurnOutput;
  static deserializeBinaryFromReader(message: TurnOutput, reader: jspb.BinaryReader): TurnOutput;
}

export namespace TurnOutput {
  export type AsObject = {
    flyList: Array<Fly.AsObject>,
  }
}

export class StarPosition extends jspb.Message {
  getX(): number;
  setX(value: number): void;

  getY(): number;
  setY(value: number): void;

  serializeBinary(): Uint8Array;
  toObject(includeInstance?: boolean): StarPosition.AsObject;
  static toObject(includeInstance: boolean, msg: StarPosition): StarPosition.AsObject;
  static extensions: {[key: number]: jspb.ExtensionFieldInfo<jspb.Message>};
  static extensionsBinary: {[key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message>};
  static serializeBinaryToWriter(message: StarPosition, writer: jspb.BinaryWriter): void;
  static deserializeBinary(bytes: Uint8Array): StarPosition;
  static deserializeBinaryFromReader(message: StarPosition, reader: jspb.BinaryReader): StarPosition;
}

export namespace StarPosition {
  export type AsObject = {
    x: number,
    y: number,
  }
}

export class Star extends jspb.Message {
  getId(): number;
  setId(value: number): void;

  getRichness(): number;
  setRichness(value: number): void;

  getOwner(): number;
  setOwner(value: number): void;

  getShipCount(): number;
  setShipCount(value: number): void;

  getTurnsToNextProduction(): number;
  setTurnsToNextProduction(value: number): void;

  serializeBinary(): Uint8Array;
  toObject(includeInstance?: boolean): Star.AsObject;
  static toObject(includeInstance: boolean, msg: Star): Star.AsObject;
  static extensions: {[key: number]: jspb.ExtensionFieldInfo<jspb.Message>};
  static extensionsBinary: {[key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message>};
  static serializeBinaryToWriter(message: Star, writer: jspb.BinaryWriter): void;
  static deserializeBinary(bytes: Uint8Array): Star;
  static deserializeBinaryFromReader(message: Star, reader: jspb.BinaryReader): Star;
}

export namespace Star {
  export type AsObject = {
    id: number,
    richness: number,
    owner: number,
    shipCount: number,
    turnsToNextProduction: number,
  }
}

export class Link extends jspb.Message {
  getStarIdA(): number;
  setStarIdA(value: number): void;

  getStarIdB(): number;
  setStarIdB(value: number): void;

  serializeBinary(): Uint8Array;
  toObject(includeInstance?: boolean): Link.AsObject;
  static toObject(includeInstance: boolean, msg: Link): Link.AsObject;
  static extensions: {[key: number]: jspb.ExtensionFieldInfo<jspb.Message>};
  static extensionsBinary: {[key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message>};
  static serializeBinaryToWriter(message: Link, writer: jspb.BinaryWriter): void;
  static deserializeBinary(bytes: Uint8Array): Link;
  static deserializeBinaryFromReader(message: Link, reader: jspb.BinaryReader): Link;
}

export namespace Link {
  export type AsObject = {
    starIdA: number,
    starIdB: number,
  }
}

export class Flight extends jspb.Message {
  getFromStarId(): number;
  setFromStarId(value: number): void;

  getToStarId(): number;
  setToStarId(value: number): void;

  getShipCount(): number;
  setShipCount(value: number): void;

  getOwner(): number;
  setOwner(value: number): void;

  getTurnsToArrival(): number;
  setTurnsToArrival(value: number): void;

  serializeBinary(): Uint8Array;
  toObject(includeInstance?: boolean): Flight.AsObject;
  static toObject(includeInstance: boolean, msg: Flight): Flight.AsObject;
  static extensions: {[key: number]: jspb.ExtensionFieldInfo<jspb.Message>};
  static extensionsBinary: {[key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message>};
  static serializeBinaryToWriter(message: Flight, writer: jspb.BinaryWriter): void;
  static deserializeBinary(bytes: Uint8Array): Flight;
  static deserializeBinaryFromReader(message: Flight, reader: jspb.BinaryReader): Flight;
}

export namespace Flight {
  export type AsObject = {
    fromStarId: number,
    toStarId: number,
    shipCount: number,
    owner: number,
    turnsToArrival: number,
  }
}

export class Fly extends jspb.Message {
  getFromStarId(): number;
  setFromStarId(value: number): void;

  getToStarId(): number;
  setToStarId(value: number): void;

  getShipCount(): number;
  setShipCount(value: number): void;

  serializeBinary(): Uint8Array;
  toObject(includeInstance?: boolean): Fly.AsObject;
  static toObject(includeInstance: boolean, msg: Fly): Fly.AsObject;
  static extensions: {[key: number]: jspb.ExtensionFieldInfo<jspb.Message>};
  static extensionsBinary: {[key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message>};
  static serializeBinaryToWriter(message: Fly, writer: jspb.BinaryWriter): void;
  static deserializeBinary(bytes: Uint8Array): Fly;
  static deserializeBinaryFromReader(message: Fly, reader: jspb.BinaryReader): Fly;
}

export namespace Fly {
  export type AsObject = {
    fromStarId: number,
    toStarId: number,
    shipCount: number,
  }
}

export class GameRecord extends jspb.Message {
  hasInitialConfiguration(): boolean;
  clearInitialConfiguration(): void;
  getInitialConfiguration(): InitialInput | undefined;
  setInitialConfiguration(value?: InitialInput): void;

  clearTurnsList(): void;
  getTurnsList(): Array<TurnInput>;
  setTurnsList(value: Array<TurnInput>): void;
  addTurns(value?: TurnInput, index?: number): TurnInput;

  clearScoresList(): void;
  getScoresList(): Array<number>;
  setScoresList(value: Array<number>): void;
  addScores(value: number, index?: number): number;

  clearFailureMessageList(): void;
  getFailureMessageList(): Array<FailureMessage>;
  setFailureMessageList(value: Array<FailureMessage>): void;
  addFailureMessage(value?: FailureMessage, index?: number): FailureMessage;

  serializeBinary(): Uint8Array;
  toObject(includeInstance?: boolean): GameRecord.AsObject;
  static toObject(includeInstance: boolean, msg: GameRecord): GameRecord.AsObject;
  static extensions: {[key: number]: jspb.ExtensionFieldInfo<jspb.Message>};
  static extensionsBinary: {[key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message>};
  static serializeBinaryToWriter(message: GameRecord, writer: jspb.BinaryWriter): void;
  static deserializeBinary(bytes: Uint8Array): GameRecord;
  static deserializeBinaryFromReader(message: GameRecord, reader: jspb.BinaryReader): GameRecord;
}

export namespace GameRecord {
  export type AsObject = {
    initialConfiguration?: InitialInput.AsObject,
    turnsList: Array<TurnInput.AsObject>,
    scoresList: Array<number>,
    failureMessageList: Array<FailureMessage.AsObject>,
  }
}

export class FailureMessage extends jspb.Message {
  getProcessId(): number;
  setProcessId(value: number): void;

  getMsg(): string;
  setMsg(value: string): void;

  serializeBinary(): Uint8Array;
  toObject(includeInstance?: boolean): FailureMessage.AsObject;
  static toObject(includeInstance: boolean, msg: FailureMessage): FailureMessage.AsObject;
  static extensions: {[key: number]: jspb.ExtensionFieldInfo<jspb.Message>};
  static extensionsBinary: {[key: number]: jspb.ExtensionFieldBinaryInfo<jspb.Message>};
  static serializeBinaryToWriter(message: FailureMessage, writer: jspb.BinaryWriter): void;
  static deserializeBinary(bytes: Uint8Array): FailureMessage;
  static deserializeBinaryFromReader(message: FailureMessage, reader: jspb.BinaryReader): FailureMessage;
}

export namespace FailureMessage {
  export type AsObject = {
    processId: number,
    msg: string,
  }
}

