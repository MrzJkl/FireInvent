import 'package:flutter/widgets.dart';
import 'package:json_annotation/json_annotation.dart';

part 'create_person_model.g.dart';

@JsonSerializable()
@immutable
class CreatePersonModel {
  final String firstName;
  final String lastName;
  final String? remarks;
  final String? contactInfo;
  final String? externalId;

  const CreatePersonModel({
    required this.firstName,
    required this.lastName,
    this.remarks,
    this.contactInfo,
    this.externalId,
  });

  factory CreatePersonModel.fromJson(Map<String, dynamic> json) =>
      _$CreatePersonModelFromJson(json);
  Map<String, dynamic> toJson() => _$CreatePersonModelToJson(this);
}
