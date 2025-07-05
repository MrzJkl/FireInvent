import 'package:flutter/material.dart';
import 'package:flutter_form_builder/flutter_form_builder.dart';
import 'package:form_builder_validators/form_builder_validators.dart';
import '../models/create_models/create_person_model.dart';
import '../models/person_model.dart';

class PersonForm extends StatefulWidget {
  final PersonModel? initial;
  final void Function(CreatePersonModel) onSubmit;

  const PersonForm({super.key, this.initial, required this.onSubmit});

  @override
  State<PersonForm> createState() => _PersonFormState();
}

class _PersonFormState extends State<PersonForm> {
  final _formKey = GlobalKey<FormBuilderState>();

  @override
  Widget build(BuildContext context) {
    return FormBuilder(
      key: _formKey,
      initialValue: {
        'firstName': widget.initial?.firstName ?? '',
        'lastName': widget.initial?.lastName ?? '',
        'contactInfo': widget.initial?.contactInfo ?? '',
        'externalId': widget.initial?.externalId ?? '',
        'remarks': widget.initial?.remarks ?? '',
      },
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          FormBuilderTextField(
            name: 'firstName',
            decoration: const InputDecoration(labelText: 'Vorname'),
            validator: FormBuilderValidators.compose([
              FormBuilderValidators.required(),
            ]),
          ),
          FormBuilderTextField(
            name: 'lastName',
            decoration: const InputDecoration(labelText: 'Nachname'),
            validator: FormBuilderValidators.compose([
              FormBuilderValidators.required(),
            ]),
          ),
          FormBuilderTextField(
            name: 'contactInfo',
            decoration: const InputDecoration(labelText: 'Kontaktinfo'),
          ),
          FormBuilderTextField(
            name: 'externalId',
            decoration: const InputDecoration(labelText: 'Externe ID'),
          ),
          FormBuilderTextField(
            name: 'remarks',
            decoration: const InputDecoration(labelText: 'Bemerkungen'),
            maxLines: 2,
          ),
          const SizedBox(height: 16),
          Row(
            mainAxisAlignment: MainAxisAlignment.end,
            children: [
              TextButton(
                onPressed: () => Navigator.of(context).pop(),
                child: const Text('Abbrechen'),
              ),
              ElevatedButton(
                onPressed: () {
                  if (_formKey.currentState?.saveAndValidate() ?? false) {
                    final values = _formKey.currentState!.value;
                    widget.onSubmit(
                      CreatePersonModel(
                        firstName: values['firstName'] as String,
                        lastName: values['lastName'] as String,
                        contactInfo:
                            (values['contactInfo'] as String).isEmpty
                                ? null
                                : values['contactInfo'] as String,
                        externalId:
                            (values['externalId'] as String).isEmpty
                                ? null
                                : values['externalId'] as String,
                        remarks:
                            (values['remarks'] as String).isEmpty
                                ? null
                                : values['remarks'] as String,
                      ),
                    );
                  }
                },
                child: const Text('Speichern'),
              ),
            ],
          ),
        ],
      ),
    );
  }
}
